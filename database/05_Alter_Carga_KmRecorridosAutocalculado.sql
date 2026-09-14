-- =====================================================================
-- App: Control de Consumo de Gasolina
-- Script: 05_Alter_Carga_KmRecorridosAutocalculado.sql
-- Agrega la columna que distingue si TB_CARGA.KILOMETROS_RECORRIDOS fue
-- calculado automáticamente (kilometraje de esta carga menos el de la
-- carga anterior) o ingresado a mano por el usuario.
--
-- Por qué: al editar o borrar una carga, la carga vecina que dependía de
-- su kilometraje para el cálculo automático queda con un valor viejo (gap
-- documentado en CLAUDE.md: "Known, intentionally deferred gap"). Ahora
-- que existe esta columna, CargaService puede recalcular esa vecina de
-- forma segura — solo cuando su valor fue autocalculado, nunca cuando el
-- usuario lo puso a mano a propósito.
--
-- Ejecutar después de 01_DDL_Tablas.sql (en bases nuevas) o en cualquier
-- momento sobre una base ya existente.
-- =====================================================================
ALTER TABLE TB_CARGA
    ADD KILOMETROS_RECORRIDOS_AUTOCALCULADO BIT NOT NULL DEFAULT 0;
GO

-- Los registros ya existentes quedan en 0 (no autocalculado) a propósito:
-- no hay forma de saber retroactivamente cuáles vinieron del cálculo
-- automático y cuáles el usuario los fijó a mano, así que se asume
-- "protegido" (no se toca solo) en vez de arriesgar sobreescribir un
-- valor real. Las cargas nuevas, y las que se vuelvan a guardar desde la
-- app de aquí en adelante, sí quedan marcadas correctamente.
