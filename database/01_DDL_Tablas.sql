-- =====================================================================
-- App: Control de Consumo de Gasolina
-- Script: 01_DDL_Tablas.sql
-- Convención: prefijo TB_, campos en MAYUSCULAS, bitácora estándar,
--             fechas en UTC (GETUTCDATE) igual que tus otros proyectos.
-- =====================================================================

-- ---------------------------------------------------------------------
-- TB_USUARIO
-- ---------------------------------------------------------------------
CREATE TABLE TB_USUARIO (
    ID                      INT IDENTITY(1,1) PRIMARY KEY,
    NOMBRE                  NVARCHAR(100)   NOT NULL,
    EMAIL                   NVARCHAR(150)   NOT NULL,
    PASSWORD_HASH           NVARCHAR(255)   NOT NULL,
    TOKEN_VERSION           INT             NOT NULL DEFAULT 1,  -- se incrementa al cambiar password; invalida JWT viejos

    -- Bitácora
    USUARIO_CREACION        INT             NULL,
    FECHA_CREACION          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    USUARIO_MODIFICACION    INT             NULL,
    FECHA_MODIFICACION      DATETIME2       NULL,
    ESTADO                  BIT             NOT NULL DEFAULT 1,  -- 1 = Activo, 0 = Eliminado
    FECHA_ELIMINACION       DATETIME2       NULL,                -- se llena al desactivar; por defecto hoy, editable

    CONSTRAINT UQ_USUARIO_EMAIL UNIQUE (EMAIL)
);
GO

-- ---------------------------------------------------------------------
-- TB_TIPO_VEHICULO  (catálogo: Sedán, Pickup, Motocicleta, SUV, etc.)
-- ---------------------------------------------------------------------
CREATE TABLE TB_TIPO_VEHICULO (
    ID                      INT IDENTITY(1,1) PRIMARY KEY,
    NOMBRE                  NVARCHAR(50)    NOT NULL,

    -- Bitácora
    USUARIO_CREACION        INT             NULL,
    FECHA_CREACION          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    USUARIO_MODIFICACION    INT             NULL,
    FECHA_MODIFICACION      DATETIME2       NULL,
    ESTADO                  BIT             NOT NULL DEFAULT 1,
    FECHA_ELIMINACION       DATETIME2       NULL,

    CONSTRAINT FK_TIPOVEH_USR_CREA FOREIGN KEY (USUARIO_CREACION)     REFERENCES TB_USUARIO(ID),
    CONSTRAINT FK_TIPOVEH_USR_MOD  FOREIGN KEY (USUARIO_MODIFICACION) REFERENCES TB_USUARIO(ID)
);
GO

-- ---------------------------------------------------------------------
-- TB_TIPO_COMBUSTIBLE  (catálogo: Regular, Súper, Diésel)
-- ---------------------------------------------------------------------
CREATE TABLE TB_TIPO_COMBUSTIBLE (
    ID                      INT IDENTITY(1,1) PRIMARY KEY,
    NOMBRE                  NVARCHAR(50)    NOT NULL,

    -- Bitácora
    USUARIO_CREACION        INT             NULL,
    FECHA_CREACION          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    USUARIO_MODIFICACION    INT             NULL,
    FECHA_MODIFICACION      DATETIME2       NULL,
    ESTADO                  BIT             NOT NULL DEFAULT 1,
    FECHA_ELIMINACION       DATETIME2       NULL,

    CONSTRAINT FK_TIPOCOMB_USR_CREA FOREIGN KEY (USUARIO_CREACION)     REFERENCES TB_USUARIO(ID),
    CONSTRAINT FK_TIPOCOMB_USR_MOD  FOREIGN KEY (USUARIO_MODIFICACION) REFERENCES TB_USUARIO(ID)
);
GO

-- ---------------------------------------------------------------------
-- TB_VEHICULO
-- ---------------------------------------------------------------------
CREATE TABLE TB_VEHICULO (
    ID                          INT IDENTITY(1,1) PRIMARY KEY,
    TIPO_VEHICULO_ID            INT             NOT NULL,
    TIPO_COMBUSTIBLE_ID         INT             NOT NULL,   -- combustible por defecto de este vehículo; se usa para autocompletar en TB_CARGA
    USUARIO_PROPIETARIO_ID      INT             NOT NULL,   -- dueño del vehículo; hoy serás siempre tú, pero queda listo para más usuarios
    NOMBRE                      NVARCHAR(50)    NOT NULL,   -- ej. "Mazda 3"
    PLACA                       NVARCHAR(20)    NULL,
    TANQUE_CAPACIDAD_GALONES    DECIMAL(5,2)    NULL,

    -- Bitácora
    USUARIO_CREACION        INT             NULL,
    FECHA_CREACION          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    USUARIO_MODIFICACION    INT             NULL,
    FECHA_MODIFICACION      DATETIME2       NULL,
    ESTADO                  BIT             NOT NULL DEFAULT 1,
    FECHA_ELIMINACION       DATETIME2       NULL,

    CONSTRAINT FK_VEHICULO_TIPO      FOREIGN KEY (TIPO_VEHICULO_ID)       REFERENCES TB_TIPO_VEHICULO(ID),
    CONSTRAINT FK_VEHICULO_COMBUSTIBLE FOREIGN KEY (TIPO_COMBUSTIBLE_ID)  REFERENCES TB_TIPO_COMBUSTIBLE(ID),
    CONSTRAINT FK_VEHICULO_PROPIETARIO FOREIGN KEY (USUARIO_PROPIETARIO_ID) REFERENCES TB_USUARIO(ID),
    CONSTRAINT FK_VEHICULO_USR_CREA  FOREIGN KEY (USUARIO_CREACION)       REFERENCES TB_USUARIO(ID),
    CONSTRAINT FK_VEHICULO_USR_MOD   FOREIGN KEY (USUARIO_MODIFICACION)   REFERENCES TB_USUARIO(ID)
);
GO

-- ---------------------------------------------------------------------
-- TB_DEPARTAMENTO  (catálogo geográfico de Guatemala)
-- ---------------------------------------------------------------------
CREATE TABLE TB_DEPARTAMENTO (
    ID                      INT IDENTITY(1,1) PRIMARY KEY,
    NOMBRE                  NVARCHAR(50)    NOT NULL,

    -- Bitácora
    USUARIO_CREACION        INT             NULL,
    FECHA_CREACION          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    USUARIO_MODIFICACION    INT             NULL,
    FECHA_MODIFICACION      DATETIME2       NULL,
    ESTADO                  BIT             NOT NULL DEFAULT 1,
    FECHA_ELIMINACION       DATETIME2       NULL,

    CONSTRAINT FK_DEPTO_USR_CREA FOREIGN KEY (USUARIO_CREACION)     REFERENCES TB_USUARIO(ID),
    CONSTRAINT FK_DEPTO_USR_MOD  FOREIGN KEY (USUARIO_MODIFICACION) REFERENCES TB_USUARIO(ID)
);
GO

-- ---------------------------------------------------------------------
-- TB_MUNICIPIO  (catálogo geográfico de Guatemala)
-- ---------------------------------------------------------------------
CREATE TABLE TB_MUNICIPIO (
    ID                      INT IDENTITY(1,1) PRIMARY KEY,
    DEPARTAMENTO_ID         INT             NOT NULL,
    NOMBRE                  NVARCHAR(100)   NOT NULL,

    -- Bitácora
    USUARIO_CREACION        INT             NULL,
    FECHA_CREACION          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    USUARIO_MODIFICACION    INT             NULL,
    FECHA_MODIFICACION      DATETIME2       NULL,
    ESTADO                  BIT             NOT NULL DEFAULT 1,
    FECHA_ELIMINACION       DATETIME2       NULL,

    CONSTRAINT FK_MUNI_DEPARTAMENTO FOREIGN KEY (DEPARTAMENTO_ID)     REFERENCES TB_DEPARTAMENTO(ID),
    CONSTRAINT FK_MUNI_USR_CREA     FOREIGN KEY (USUARIO_CREACION)     REFERENCES TB_USUARIO(ID),
    CONSTRAINT FK_MUNI_USR_MOD      FOREIGN KEY (USUARIO_MODIFICACION) REFERENCES TB_USUARIO(ID)
);
GO

-- ---------------------------------------------------------------------
-- TB_ESTACION_SERVICIO  (catálogo que crece solo desde el formulario de carga)
-- ---------------------------------------------------------------------
CREATE TABLE TB_ESTACION_SERVICIO (
    ID                      INT IDENTITY(1,1) PRIMARY KEY,
    MUNICIPIO_ID             INT             NULL,       -- el departamento se obtiene por JOIN a través del municipio, no se duplica aquí
    NOMBRE                  NVARCHAR(100)   NOT NULL,    -- ej. "Shell 6ta Avenida"
    MARCA                   NVARCHAR(50)    NULL,        -- ej. "Shell", "Texaco", "Puma", "Esso"
    LATITUD                 DECIMAL(9,6)    NULL,        -- capturado a mano (ej. copiado de Google Maps), no por GPS del dispositivo
    LONGITUD                DECIMAL(9,6)    NULL,
    REFERENCIA              NVARCHAR(255)   NULL,        -- ej. "frente al Walmart Vista Hermosa"

    -- Bitácora
    USUARIO_CREACION        INT             NULL,
    FECHA_CREACION          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    USUARIO_MODIFICACION    INT             NULL,
    FECHA_MODIFICACION      DATETIME2       NULL,
    ESTADO                  BIT             NOT NULL DEFAULT 1,
    FECHA_ELIMINACION       DATETIME2       NULL,

    CONSTRAINT FK_ESTACION_MUNICIPIO FOREIGN KEY (MUNICIPIO_ID)         REFERENCES TB_MUNICIPIO(ID),
    CONSTRAINT FK_ESTACION_USR_CREA  FOREIGN KEY (USUARIO_CREACION)     REFERENCES TB_USUARIO(ID),
    CONSTRAINT FK_ESTACION_USR_MOD   FOREIGN KEY (USUARIO_MODIFICACION) REFERENCES TB_USUARIO(ID)
);
GO

-- ---------------------------------------------------------------------
-- TB_CARGA
-- ---------------------------------------------------------------------
CREATE TABLE TB_CARGA (
    ID                          INT IDENTITY(1,1) PRIMARY KEY,
    VEHICULO_ID                 INT             NOT NULL,
    TIPO_COMBUSTIBLE_ID         INT             NOT NULL,   -- se autocompleta con el del vehículo al seleccionar, pero queda congelado en esta carga
    ESTACION_SERVICIO_ID        INT             NULL,       -- nullable: puedes registrar una carga sin especificar dónde
    FECHA                       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),  -- default hoy, editable al registrar carga pasada
    KILOMETRAJE                 DECIMAL(10,2)   NOT NULL,   -- lo que marca el odómetro en ese momento
    KILOMETROS_RECORRIDOS       DECIMAL(10,2)   NULL,       -- km desde la última carga, si lo sabes (si no, la app lo calcula con el KILOMETRAJE anterior)
    GALONES                     DECIMAL(6,2)    NOT NULL,
    COSTO_TOTAL                 DECIMAL(8,2)    NOT NULL,   -- lo que pagaste en total por esos galones
    PRECIO_POR_GALON            AS (CASE WHEN GALONES = 0 THEN NULL ELSE COSTO_TOTAL / GALONES END) PERSISTED,
    LITROS                      AS (GALONES * 3.785411784) PERSISTED,                          -- 1 galón US = 3.785411784 litros
    PRECIO_POR_LITRO            AS (CASE WHEN GALONES = 0 THEN NULL ELSE COSTO_TOTAL / (GALONES * 3.785411784) END) PERSISTED,

    -- Bitácora
    USUARIO_CREACION        INT             NULL,
    FECHA_CREACION          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    USUARIO_MODIFICACION    INT             NULL,
    FECHA_MODIFICACION      DATETIME2       NULL,
    ESTADO                  BIT             NOT NULL DEFAULT 1,
    FECHA_ELIMINACION       DATETIME2       NULL,

    CONSTRAINT FK_CARGA_VEHICULO    FOREIGN KEY (VEHICULO_ID)          REFERENCES TB_VEHICULO(ID),
    CONSTRAINT FK_CARGA_COMBUSTIBLE FOREIGN KEY (TIPO_COMBUSTIBLE_ID)  REFERENCES TB_TIPO_COMBUSTIBLE(ID),
    CONSTRAINT FK_CARGA_ESTACION    FOREIGN KEY (ESTACION_SERVICIO_ID) REFERENCES TB_ESTACION_SERVICIO(ID),
    CONSTRAINT FK_CARGA_USR_CREA    FOREIGN KEY (USUARIO_CREACION)     REFERENCES TB_USUARIO(ID),
    CONSTRAINT FK_CARGA_USR_MOD     FOREIGN KEY (USUARIO_MODIFICACION) REFERENCES TB_USUARIO(ID)
);
GO

-- ---------------------------------------------------------------------
-- TB_LOG_AUDITORIA  (bitácora general: qué se hizo, quién, desde dónde)
-- ---------------------------------------------------------------------
CREATE TABLE TB_LOG_AUDITORIA (
    ID                  BIGINT IDENTITY(1,1) PRIMARY KEY,  -- BIGINT: esta tabla crece rápido, nunca se borra
    USUARIO_ID          INT             NULL,               -- NULL si fue un intento de login fallido, por ejemplo
    FECHA               DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    TIPO_ACCION         NVARCHAR(20)    NOT NULL,            -- INSERT, UPDATE, DELETE, SELECT
    ENTIDAD             NVARCHAR(100)   NOT NULL,            -- tabla/entidad afectada, ej. TB_CARGA
    ID_REGISTRO         NVARCHAR(50)    NULL,                -- ID usado (SELECT/UPDATE/DELETE) o el ID generado (INSERT)
    PARAMETROS          NVARCHAR(MAX)   NULL,                -- JSON con los parámetros de entrada de la petición
    CONTROLADOR         NVARCHAR(100)   NULL,                -- ej. "CargasController"
    ACCION_METODO       NVARCHAR(100)   NULL,                -- ej. "RegistrarCarga", "ObtenerHistorico"
    VISTA_ORIGEN        NVARCHAR(100)   NULL,                -- pantalla de React desde donde se originó (ver nota abajo)
    EXITOSO             BIT             NOT NULL DEFAULT 1,
    MENSAJE_ERROR       NVARCHAR(500)   NULL,

    CONSTRAINT FK_LOGAUD_USUARIO FOREIGN KEY (USUARIO_ID) REFERENCES TB_USUARIO(ID)
    -- Sin bitácora propia ni ESTADO: este registro es de solo lectura una vez escrito, no se edita ni se borra.
);
GO
-- ---------------------------------------------------------------------
-- Catálogos iniciales
-- ---------------------------------------------------------------------
INSERT INTO TB_TIPO_VEHICULO (NOMBRE) VALUES
    (N'Sedán'), (N'Pickup'), (N'SUV'), (N'Motocicleta'), (N'Furgoneta');

INSERT INTO TB_TIPO_COMBUSTIBLE (NOMBRE) VALUES
    (N'Regular'), (N'Súper'), (N'Diésel');
GO

-- =====================================================================
-- Índices: llaves foráneas, filtros frecuentes y unicidad condicionada
-- =====================================================================

-- FKs (SQL Server no las indexa automáticamente)
CREATE INDEX IX_VEHICULO_TIPO         ON TB_VEHICULO (TIPO_VEHICULO_ID);
CREATE INDEX IX_VEHICULO_COMBUSTIBLE  ON TB_VEHICULO (TIPO_COMBUSTIBLE_ID);
CREATE INDEX IX_VEHICULO_PROPIETARIO  ON TB_VEHICULO (USUARIO_PROPIETARIO_ID);
CREATE INDEX IX_CARGA_VEHICULO        ON TB_CARGA (VEHICULO_ID);
CREATE INDEX IX_CARGA_COMBUSTIBLE     ON TB_CARGA (TIPO_COMBUSTIBLE_ID);
CREATE INDEX IX_CARGA_ESTACION        ON TB_CARGA (ESTACION_SERVICIO_ID);
CREATE INDEX IX_MUNICIPIO_DEPARTAMENTO ON TB_MUNICIPIO (DEPARTAMENTO_ID);
CREATE INDEX IX_ESTACION_MUNICIPIO     ON TB_ESTACION_SERVICIO (MUNICIPIO_ID);

-- Filtro por rango de fechas (estadísticas por mes/últimos N meses)
CREATE INDEX IX_CARGA_FECHA           ON TB_CARGA (FECHA);

-- Casi todas las consultas llevarán WHERE ESTADO = 1: índice filtrado, más liviano
CREATE INDEX IX_VEHICULO_ACTIVOS ON TB_VEHICULO (ID) WHERE ESTADO = 1;
CREATE INDEX IX_CARGA_ACTIVAS    ON TB_CARGA (ID)    WHERE ESTADO = 1;

-- Placa única solo entre vehículos activos (con soft-delete, un UNIQUE normal
-- bloquearía reusar la placa de un vehículo ya eliminado)
CREATE UNIQUE INDEX UQ_VEHICULO_PLACA_ACTIVA
    ON TB_VEHICULO (PLACA)
    WHERE ESTADO = 1 AND PLACA IS NOT NULL;

-- TB_LOG_AUDITORIA: consultas típicas son "qué hizo este usuario" y
-- "qué pasó con este registro"
CREATE INDEX IX_LOGAUD_FECHA    ON TB_LOG_AUDITORIA (FECHA);
CREATE INDEX IX_LOGAUD_USUARIO  ON TB_LOG_AUDITORIA (USUARIO_ID, FECHA);
CREATE INDEX IX_LOGAUD_ENTIDAD  ON TB_LOG_AUDITORIA (ENTIDAD, ID_REGISTRO);
GO
