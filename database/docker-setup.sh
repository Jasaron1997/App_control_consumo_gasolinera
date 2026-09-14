#!/usr/bin/env bash
# Prepara la instancia local de SQL Server levantada por docker-compose.yml:
# crea la base GasolinaDb, corre 01_DDL_Tablas.sql + 02_Seed_Geografia.sql,
# crea el login de bajo privilegio (equivalente a 04_Crear_Login_App.sql) y
# siembra tu usuario de prueba generando un hash de BCrypt real (no el
# ejemplo de 03_Seed_Usuario.sql). Pensado para poder correrse varias veces
# sin romper nada: cada paso se salta si ya se aplicó antes.
#
# Uso:
#   docker compose up -d
#   ./database/docker-setup.sh
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

if [ ! -f .env ]; then
  echo "Falta .env — corré: cp .env.example .env" >&2
  exit 1
fi
set -a
# shellcheck disable=SC1091
source .env
set +a

SQLCMD=(docker compose exec -T db /opt/mssql-tools18/bin/sqlcmd -C -f 65001)

# Duplica comillas simples para incrustar valores de forma segura en literales
# T-SQL (ej. si SEED_USUARIO_NOMBRE trajera un apóstrofe).
escapar_sql() { printf '%s' "${1//\'/\'\'}"; }

echo "Esperando a que SQL Server acepte conexiones..."
until "${SQLCMD[@]}" -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1" -b -o /dev/null 2>/dev/null; do
  sleep 2
done
echo "SQL Server listo."

existe_bd=$("${SQLCMD[@]}" -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -h -1 -Q "SET NOCOUNT ON; SELECT CASE WHEN DB_ID('GasolinaDb') IS NULL THEN 0 ELSE 1 END" | tr -d '[:space:]')

if [ "$existe_bd" != "1" ]; then
  echo "Creando base GasolinaDb..."
  "${SQLCMD[@]}" -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "CREATE DATABASE GasolinaDb"
else
  echo "GasolinaDb ya existe, se omite CREATE DATABASE."
fi

existe_schema=$("${SQLCMD[@]}" -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d GasolinaDb -h -1 -Q "SET NOCOUNT ON; SELECT CASE WHEN OBJECT_ID('dbo.TB_USUARIO') IS NULL THEN 0 ELSE 1 END" | tr -d '[:space:]')

if [ "$existe_schema" != "1" ]; then
  echo "Aplicando 01_DDL_Tablas.sql..."
  # ANSI_NULLS/QUOTED_IDENTIFIER deben estar ON en la sesión para crear TB_CARGA
  # (columnas PERSISTED) y el índice único filtrado de TB_VEHICULO.PLACA; sqlcmd
  # no los pone ON por defecto.
  { printf 'SET ANSI_NULLS ON;\nSET QUOTED_IDENTIFIER ON;\nGO\n'; cat database/01_DDL_Tablas.sql; } \
    | "${SQLCMD[@]}" -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d GasolinaDb

  echo "Aplicando 02_Seed_Geografia.sql..."
  "${SQLCMD[@]}" -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d GasolinaDb < database/02_Seed_Geografia.sql
else
  echo "El esquema ya existe (TB_USUARIO ya está creada), se omiten 01 y 02."
fi

existe_columna_autocalculado=$("${SQLCMD[@]}" -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d GasolinaDb -h -1 \
  -Q "SET NOCOUNT ON; SELECT CASE WHEN COL_LENGTH('dbo.TB_CARGA', 'KILOMETROS_RECORRIDOS_AUTOCALCULADO') IS NULL THEN 0 ELSE 1 END" | tr -d '[:space:]')

if [ "$existe_columna_autocalculado" != "1" ]; then
  echo "Aplicando 05_Alter_Carga_KmRecorridosAutocalculado.sql..."
  "${SQLCMD[@]}" -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d GasolinaDb < database/05_Alter_Carga_KmRecorridosAutocalculado.sql
else
  echo "TB_CARGA.KILOMETROS_RECORRIDOS_AUTOCALCULADO ya existe, se omite 05."
fi

echo "Generando hash de BCrypt para SEED_USUARIO_PASSWORD (dotnet fsi, mismo método que documenta 03_Seed_Usuario.sql)..."
# mktemp -t en macOS APPEND un sufijo aleatorio al final del nombre dado (no
# sustituye XXXXXX), así que el archivo no terminaría en .fsx y dotnet fsi no
# lo reconocería como script — se usa un directorio temporal en su lugar.
hash_dir="$(mktemp -d)"
trap 'rm -rf "$hash_dir"' EXIT
hash_fsx="$hash_dir/hash.fsx"
cat > "$hash_fsx" <<'EOF'
#r "nuget: BCrypt.Net-Next, 4.0.3"
let password = fsi.CommandLineArgs.[1]
printfn "%s" (BCrypt.Net.BCrypt.HashPassword(password))
EOF
hash_bcrypt=$(dotnet fsi "$hash_fsx" -- "$SEED_USUARIO_PASSWORD")

email_sql="$(escapar_sql "$SEED_USUARIO_EMAIL")"
nombre_sql="$(escapar_sql "$SEED_USUARIO_NOMBRE")"

existe_usuario=$("${SQLCMD[@]}" -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d GasolinaDb -h -1 \
  -Q "SET NOCOUNT ON; SELECT CASE WHEN EXISTS (SELECT 1 FROM TB_USUARIO WHERE EMAIL = N'$email_sql') THEN 1 ELSE 0 END" | tr -d '[:space:]')

if [ "$existe_usuario" != "1" ]; then
  echo "Sembrando usuario de prueba ($SEED_USUARIO_EMAIL)..."
  "${SQLCMD[@]}" -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d GasolinaDb -Q \
    "INSERT INTO TB_USUARIO (NOMBRE, EMAIL, PASSWORD_HASH, TOKEN_VERSION, ESTADO) VALUES (N'$nombre_sql', N'$email_sql', N'$hash_bcrypt', 1, 1)"
else
  echo "El usuario $SEED_USUARIO_EMAIL ya existe, se omite el INSERT."
fi

existe_login=$("${SQLCMD[@]}" -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -h -1 \
  -Q "SET NOCOUNT ON; SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'gasolina_api') THEN 1 ELSE 0 END" | tr -d '[:space:]')

if [ "$existe_login" != "1" ]; then
  echo "Creando login gasolina_api (equivalente a 04_Crear_Login_App.sql)..."
  "${SQLCMD[@]}" -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q \
    "CREATE LOGIN gasolina_api WITH PASSWORD = N'$(escapar_sql "$GASOLINA_API_DB_PASSWORD")'"
  "${SQLCMD[@]}" -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d GasolinaDb -Q \
    "CREATE USER gasolina_api FOR LOGIN gasolina_api; ALTER ROLE db_datareader ADD MEMBER gasolina_api; ALTER ROLE db_datawriter ADD MEMBER gasolina_api;"
else
  echo "El login gasolina_api ya existe, se omite."
fi

cat <<EOF

Listo. Para apuntar GasolinaApi a esta instancia local:

  cd GasolinaApi
  dotnet user-secrets set "ConnectionStrings:GasolinaDb" "Server=localhost;Database=GasolinaDb;User Id=gasolina_api;Password=$GASOLINA_API_DB_PASSWORD;TrustServerCertificate=True;"
  dotnet user-secrets set "Jwt:Secret" "<genera-una-clave-de-32-caracteres-o-mas>"

Login de prueba (POST /api/auth/login):
  email:    $SEED_USUARIO_EMAIL
  password: $SEED_USUARIO_PASSWORD

Para conectarte con una herramienta (Azure Data Studio, DBeaver, etc.):
  Server: localhost,1433
  User:   sa
  Pass:   (la de MSSQL_SA_PASSWORD en tu .env)
EOF
