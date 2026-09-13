-- =====================================================================
-- App: Control de Consumo de Gasolina
-- Script: 04_Crear_Login_App.sql
-- Crea un login de SQL con permisos mínimos (solo lectura/escritura de
-- datos, sin permisos de administración) para que la API se conecte con
-- él en vez de con el usuario admin de Azure SQL.
--
-- Ejecutar en dos pasos:
--   1) La parte de CREATE LOGIN, conectado a la base 'master'.
--   2) La parte de CREATE USER + roles, conectado a la base GasolinaDb.
-- =====================================================================

-- Paso 1: en la base 'master' (cambia la contraseña por una fuerte y única)
-- CREATE LOGIN gasolina_api WITH PASSWORD = 'CAMBIA-ESTA-CONTRASEÑA-0!';

-- Paso 2: en la base GasolinaDb
CREATE USER gasolina_api FOR LOGIN gasolina_api;
ALTER ROLE db_datareader ADD MEMBER gasolina_api;
ALTER ROLE db_datawriter ADD MEMBER gasolina_api;
GO
