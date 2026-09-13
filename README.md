# Control de Consumo de Gasolina

App personal (un solo usuario) para registrar cargas de combustible y ver
estadísticas de consumo: rendimiento, gasto mensual, costo por galón/litro
y costo por kilómetro.

## Stack

- **Backend**: .NET 8 Web API (`GasolinaApi/`)
- **Frontend**: React + Vite (`gasolina-app/`)
- **Base de datos**: SQL Server / Azure SQL Database (`database/`)
- **Autenticación**: JWT, un único usuario, sin registro abierto

## Estructura del repositorio

```
.
├── GasolinaApi/        Backend .NET 8 Web API
├── gasolina-app/        Frontend React (Vite)
└── database/             Scripts SQL, en orden de ejecución
    ├── 01_DDL_Tablas.sql          Esquema completo (tablas, índices, catálogos fijos)
    ├── 02_Seed_Geografia.sql      Departamentos y municipios de Guatemala
    ├── 03_Seed_Usuario.sql        Plantilla para crear tu único usuario
    └── 04_Crear_Login_App.sql     Login de SQL de bajo privilegio para la API
```

**Enfoque de base de datos: Database First.** El esquema en
`01_DDL_Tablas.sql` se ejecuta tal cual contra SQL Server / Azure SQL; el
proyecto **no usa migraciones de EF Core** (no hay carpeta `Migrations/`
ni se corre `dotnet ef migrations add`). Las clases en `Models/` y los
mapeos en `Data/Configurations/` describen ese esquema ya existente, pero
nunca lo generan ni lo modifican. Si el esquema cambia, el cambio se hace
con un nuevo script SQL y luego se ajustan los mapeos de EF a mano.

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) y npm
- Un SQL Server accesible: SQL Server local/Docker para desarrollo, o
  Azure SQL Database (ver sección de despliegue)

## 1. Base de datos

Ejecuta los scripts de `database/` **en orden**, contra tu instancia de
SQL Server (con SQL Server Management Studio, Azure Data Studio, o
`sqlcmd`):

1. `01_DDL_Tablas.sql` — crea todas las tablas, índices y los catálogos
   fijos de tipo de vehículo/combustible.
2. `02_Seed_Geografia.sql` — carga los 22 departamentos y sus municipios.
   Se compiló a partir de fuentes públicas; si notas algún municipio
   faltante o mal escrito, agrégalo o corrígelo con un `INSERT`/`UPDATE`
   normal a `TB_MUNICIPIO`.
3. `03_Seed_Usuario.sql` — abre este archivo y sigue las instrucciones en
   los comentarios para generar el hash de tu contraseña con BCrypt antes
   de ejecutarlo (no hay registro abierto: este es el único usuario de la
   app y se crea manualmente una sola vez).
4. `04_Crear_Login_App.sql` — (recomendado, sobre todo en Azure) crea un
   login de SQL con permisos mínimos (`db_datareader` + `db_datawriter`)
   para que la API nunca se conecte con el usuario administrador.

## 2. Backend (GasolinaApi) en local

El proyecto no guarda secretos en `appsettings.json` (queda vacío/placeholder
en el repo). Para desarrollo local se usan **user-secrets** de .NET, que
viven fuera del repositorio:

```bash
cd GasolinaApi
dotnet user-secrets set "ConnectionStrings:GasolinaDb" "Server=localhost;Database=GasolinaDb;User Id=gasolina_api;Password=TU_PASSWORD;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Secret" "una-cadena-larga-y-aleatoria-de-al-menos-32-caracteres"
dotnet run
```

La API queda escuchando en `http://localhost:5114` (ver
`Properties/launchSettings.json`). Swagger está disponible en
`http://localhost:5114/swagger` en modo Development.

`appsettings.Development.json` ya trae `Cors:OrigenesPermitidos` apuntando
a `http://localhost:5173` (el puerto por defecto de Vite), así que el
frontend local puede llamar a la API sin configuración adicional.

## 3. Frontend (gasolina-app) en local

```bash
cd gasolina-app
npm install
npm run dev
```

Por defecto usa `VITE_API_URL=http://localhost:5114` (ver `.env`). Abre
`http://localhost:5173`, inicia sesión con el usuario que creaste en el
paso 1.3.

## Despliegue a Azure (capa gratis)

Resumen del objetivo: backend en **Azure App Service (plan F1)**, frontend
en **Azure Static Web Apps**, base de datos en **Azure SQL Database (capa
gratis permanente)** — costo total: $0.

### 4.1 Azure SQL Database

1. Crea un servidor lógico de Azure SQL y una base de datos en la oferta
   gratis permanente ("Free offer", no el trial de crédito).
2. Conéctate con el usuario administrador y ejecuta los 4 scripts de
   `database/` en orden (igual que en local).
3. Anota la cadena de conexión usando el login de bajo privilegio
   (`gasolina_api`) creado en el paso 4, no el administrador:
   ```
   Server=tcp:<tu-servidor>.database.windows.net,1433;Database=GasolinaDb;User Id=gasolina_api;Password=<password>;Encrypt=True;TrustServerCertificate=False;
   ```

### 4.2 Backend → Azure App Service (plan F1)

1. Crea un App Service Linux o Windows en el plan **F1 (gratis)**, stack
   **.NET 8**.
2. Configura estas **Application settings** (Configuration → Application
   settings) — el doble guion bajo `__` mapea a la jerarquía anidada de
   `appsettings.json`:
   - `ConnectionStrings__GasolinaDb` → la cadena de conexión de Azure SQL
     del paso anterior.
   - `Jwt__Secret` → una cadena aleatoria larga (guárdala también en un
     lugar seguro; si la cambias, invalidas todas las sesiones).
   - `Jwt__Issuer`, `Jwt__Audience` → puedes dejar los valores por defecto
     de `appsettings.json` (`GasolinaApi` / `GasolinaApp`) o personalizarlos.
   - `Cors__OrigenesPermitidos__0` → la URL pública de tu Static Web App
     (ej. `https://tu-app.azurestaticapps.net`).
   - `ASPNETCORE_ENVIRONMENT` → `Production`.
3. Publica el proyecto `GasolinaApi/` (Visual Studio "Publish", `dotnet
   publish` + despliegue por zip/FTP, o un pipeline de CI/CD) al App
   Service.
4. El plan F1 no tiene "Always On"; la primera petición después de estar
   inactivo puede tardar unos segundos (cold start) — normal y esperado
   para una app personal de costo cero.

### 4.3 Frontend → Azure Static Web Apps

1. Crea un recurso de Azure Static Web Apps (capa gratis) apuntando a tu
   repositorio; configúralo con:
   - **App location**: `gasolina-app`
   - **Output location**: `dist`
2. En la configuración de build (o como variable de entorno en el
   workflow de GitHub Actions que Azure genera automáticamente), define
   `VITE_API_URL` con la URL pública de tu App Service
   (ej. `https://tu-api.azurewebsites.net`), para que quede embebida en
   el build de producción.
3. Una vez desplegado, confirma que `Cors__OrigenesPermitidos__0` en el
   backend (paso 4.2) coincide exactamente con la URL que te asignó
   Static Web Apps.

## Notas de seguridad

- El backend nunca debe conectarse a Azure SQL con el usuario
  administrador; usa siempre el login de bajo privilegio del script
  `04_Crear_Login_App.sql`.
- `Jwt:Secret` y la cadena de conexión son secretos: nunca los subas al
  repositorio. En local usa `dotnet user-secrets`; en Azure, las
  Application settings del App Service (o, para un nivel extra de
  seguridad, Azure Key Vault referenciado desde ahí).
- Cambiar el password del usuario (`TB_USUARIO.PASSWORD_HASH`) sin
  incrementar `TOKEN_VERSION` en la misma actualización no invalida las
  sesiones ya emitidas; incrementa siempre ambos juntos.
