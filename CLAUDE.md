# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Personal (single-user) fuel-consumption tracking app: .NET 8 Web API backend
(`GasolinaApi/`), React + Vite frontend (`gasolina-app/`), SQL Server/Azure SQL
database (`database/`). There is no open registration — exactly one user exists,
created manually via `database/03_Seed_Usuario.sql`.

## Commands

### Backend (`GasolinaApi/`)

```bash
cd GasolinaApi
dotnet build
dotnet run                      # listens on http://localhost:5114 (see Properties/launchSettings.json)
```

Local secrets (never stored in `appsettings.json`, which ships with empty
placeholders) are set via .NET user-secrets:

```bash
dotnet user-secrets set "ConnectionStrings:GasolinaDb" "Server=localhost;Database=GasolinaDb;User Id=gasolina_api;Password=...;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Secret" "a-long-random-string-at-least-32-chars"
```

There is no test project in this repo — there are no unit/integration tests to run.
Swagger UI is available at `http://localhost:5114/swagger` only in Development.

### Frontend (`gasolina-app/`)

```bash
cd gasolina-app
npm install
npm run dev       # Vite dev server on http://localhost:5173
npm run build     # production build to dist/
npm run lint      # oxlint
```

`VITE_API_URL` (see `.env`) points the frontend at the backend; defaults to
`http://localhost:5114`. `appsettings.Development.json` already allows CORS
from `http://localhost:5173`, so local dev works with no extra config.

### Database (`database/`)

Run the 4 scripts **in order** against SQL Server (sqlcmd, Azure Data Studio,
SSMS): `01_DDL_Tablas.sql` → `02_Seed_Geografia.sql` → `03_Seed_Usuario.sql`
(follow the in-file comments to BCrypt-hash the password before running it) →
`04_Crear_Login_App.sql` (low-privilege SQL login for the API).

## Architecture

### Database First — hard constraint

`database/01_DDL_Tablas.sql` is the source of truth and is run as-is against
SQL Server. **This project does not use EF Core migrations** — there is no
`Migrations/` folder and `dotnet ef migrations add` must never be run. EF
models (`Models/`) and mappings (`Data/Configurations/`, one
`IEntityTypeConfiguration<T>` per table) describe the existing schema; they
never generate or alter it. If the schema needs to change, write a new SQL
script first and hand-adjust the EF mappings to match.

### Backend request pipeline

Layering: `Controllers/` → `Services/` (business logic) → `DTOs/`
(Requests/Responses) → `Models/` (EF entities) → `Data/`
(`AppDbContext` + `Configurations/`). Every entity inherits `EntidadBase`
(`Id`, `UsuarioCreacion`, `FechaCreacion`, `UsuarioModificacion`,
`FechaModificacion`, `Estado`, `FechaEliminacion`) — deletes are soft
(`Estado = false`), never physical row removal.

Pipeline order, registered in `Program.cs`: `ManejoErroresMiddleware` (maps
`NoEncontradoException`→404, `ValidacionException`→400,
`UnauthorizedAccessException`→401, anything else→generic 500, all wrapped in
`RespuestaApi<T>`) → CORS → Authentication → Authorization. Two pieces run
*before* any normal `ActionFilter` gets a chance, so each has to independently
produce the same `RespuestaApi<T>` shape and its own audit-log entry:
- `ValidarTokenVersionFilter` (`IAsyncAuthorizationFilter`): compares the
  JWT's `TokenVersion` claim against `TB_USUARIO.TOKEN_VERSION` (and
  `Estado`); a mismatch means the session was invalidated (password change,
  deactivation) and returns 401.
- `ValidacionModeloResponseFactory` (wired via
  `ConfigureApiBehaviorOptions().InvalidModelStateResponseFactory`): replaces
  `[ApiController]`'s automatic 400 (which normally bypasses
  `RespuestaApi<T>` and audit logging entirely).

Every other controller action goes through the global `AuditoriaActionFilter`
(`IAsyncActionFilter`), which writes one row to `TB_LOG_AUDITORIA` per
request (skippable with `[SinAuditoria]`). All three of the above audit
writers build their `LogAuditoria` row through the shared
`AuditoriaHelper.CrearLog` factory rather than constructing it inline —
extend that factory, don't re-duplicate its field list, if a 4th call site
needs one.

`RespuestaApi<T>` (`Exito`, `Datos`, `Mensaje`) is the response envelope for
every endpoint, success or failure — never return a bare DTO or the
framework's default `ProblemDetails`.

### EF Core projection gotcha

`CargaService` and `VehiculoService` project entities to response DTOs via a
`static readonly Expression<Func<TEntity, TResponse>>` field (e.g.
`ProyeccionRespuesta`), passed directly to `.Select(...)`. This is required,
not stylistic: a `.Select(x => SomeMethod(x))` call is **not** translated to
SQL by EF Core — it silently falls back to fetching the bare entity without
its navigation properties and evaluates the method client-side, which
NullReferenceExceptions on any `!`-asserted navigation property. Keep new
projections as expression-tree fields, not method calls passed into `Select`.

### Local-date handling

`TB_CARGA.FECHA` is stored as a local calendar date with no offset
(`DateTimeKind.Unspecified`, e.g. `2026-08-31T00:00:00`) — matching
`fechaLocalHoy()` on the frontend. Anything that needs "today" to compare
against that column must use `GasolinaApi.Common.FechaLocal.Hoy`, not
`DateTime.Now`/`DateTime.UtcNow` directly: the app is single-user in
Guatemala (fixed UTC-6, no DST), so `FechaLocal` hardcodes that offset rather
than trusting the host's configured timezone (which is typically UTC in
containers/cloud hosting and would otherwise shift "today" by a day for
several hours each evening).

### Auth / sessions

JWT bearer auth with an extra `TokenVersion` claim (`ClaimsGasolina`). Login
(`AuthService.LoginAsync`) always runs `BCrypt.Verify` — even when the email
doesn't exist, it verifies against a fixed dummy hash — to avoid a timing
side-channel for user enumeration. Changing the password
(`AuthService.CambiarPasswordAsync`) increments `TB_USUARIO.TOKEN_VERSION`,
which invalidates every previously-issued JWT (including the one used to
make that same request) on its next request, via
`ValidarTokenVersionFilter`. Always bump `TokenVersion` and `PasswordHash`
together.

### Frontend structure

`src/api/httpClient.js` is a single axios instance with request/response
interceptors: attaches `Authorization: Bearer <token>` and an
`X-Vista-Origen` header (set via `setVistaOrigen`, consumed for audit
logging) to every request, and on a 401 response clears the stored token and
calls a registered `onNoAutorizado` callback. `src/context/AuthContext.jsx` +
`src/components/RutaProtegida.jsx` gate routes on that token. Page-level
`<select>`-driven vehicle filters (`Dashboard.jsx`, `Historial.jsx`) share the
`SelectorVehiculo` component; the free-text station picker with
create-on-the-fly behavior is `SelectorEstacion`.

### Known, intentionally deferred gap

Editing or soft-deleting a `Carga` does **not** recompute the
auto-calculated `KilometrosRecorridos` stored on the chronologically
adjacent `Carga` that was derived from it. This has been identified multiple
times and left unfixed on purpose — it needs a product decision (cascade
recalculation? mark the neighbor as stale? a flag distinguishing
user-entered vs. computed values?) rather than a unilateral backend change.
