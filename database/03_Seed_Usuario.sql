-- =====================================================================
-- App: Control de Consumo de Gasolina
-- Script: 03_Seed_Usuario.sql
-- Crea el único usuario de la aplicación. No hay registro abierto
-- (POST /api/auth/login es el único endpoint público), así que este
-- usuario se crea manualmente, una sola vez, con este script.
--
-- CÓMO GENERAR TU HASH DE BCRYPT (nunca pongas tu contraseña en texto
-- plano en este archivo ni la subas a ningún repositorio):
--
--   Con el SDK de .NET ya instalado (el mismo que usas para GasolinaApi),
--   corre esto en una terminal (te pedirá la contraseña y te imprimirá el
--   hash; no requiere tocar el proyecto ni instalar nada adicional):
--
--     cat > /tmp/hash.fsx << 'EOF'
--     open System
--     Console.Write("Contraseña: ")
--     let password = Console.ReadLine()
--     #r "nuget: BCrypt.Net-Next, 4.0.3"
--     printfn "%s" (BCrypt.Net.BCrypt.HashPassword(password))
--     EOF
--     dotnet fsi /tmp/hash.fsx
--
--   Copia el hash que imprime (empieza con "$2a$" o "$2b$") y pégalo abajo
--   en PASSWORD_HASH.
--
-- El valor de abajo ('$2a$11$K3nN5rF3x1q8v8b8s8z8zOeYQh3vL3dQqj8qf6m0mG8YV2n5k9qgy')
-- es SOLO UN EJEMPLO DE FORMATO (hash de BCrypt de una cadena cualquiera),
-- no corresponde a ninguna contraseña real. Reemplázalo por el hash que
-- generaste con tu propia contraseña antes de ejecutar este script.
-- =====================================================================

INSERT INTO TB_USUARIO (NOMBRE, EMAIL, PASSWORD_HASH, TOKEN_VERSION, ESTADO)
VALUES (
    N'Tu Nombre',                                                    -- <-- cambia esto
    N'tu-email@ejemplo.com',                                         -- <-- cambia esto
    N'$2a$11$K3nN5rF3x1q8v8b8s8z8zOeYQh3vL3dQqj8qf6m0mG8YV2n5k9qgy',  -- <-- reemplaza con TU hash BCrypt real
    1,
    1
);
GO

-- Para invalidar todas las sesiones activas (ej. después de cambiar tu
-- contraseña manualmente en la base de datos), incrementa TOKEN_VERSION:
--
--   UPDATE TB_USUARIO SET TOKEN_VERSION = TOKEN_VERSION + 1, PASSWORD_HASH = N'<nuevo-hash>'
--   WHERE EMAIL = N'tu-email@ejemplo.com';
