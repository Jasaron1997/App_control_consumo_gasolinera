using GasolinaApi.Data;
using GasolinaApi.DTOs.Requests;
using GasolinaApi.DTOs.Responses;
using GasolinaApi.Exceptions;
using GasolinaApi.Models;
using GasolinaApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GasolinaApi.Services;

public class CargaService : ICargaService
{
    private readonly AppDbContext _dbContext;

    public CargaService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CargaResponse>> ObtenerAsync(int usuarioId, int? vehiculoId, DateTime? desde, DateTime? hasta)
    {
        var consulta = _dbContext.Cargas
            .Where(c => c.Estado && c.Vehiculo!.UsuarioPropietarioId == usuarioId);

        if (vehiculoId is not null)
        {
            consulta = consulta.Where(c => c.VehiculoId == vehiculoId);
        }

        if (desde is not null)
        {
            consulta = consulta.Where(c => c.Fecha >= desde);
        }

        if (hasta is not null)
        {
            consulta = consulta.Where(c => c.Fecha <= hasta);
        }

        return await consulta
            .OrderByDescending(c => c.Fecha)
            .Select(c => ProyectarRespuesta(c))
            .ToListAsync();
    }

    public async Task<CargaResponse> CrearAsync(CrearCargaRequest request, int usuarioId)
    {
        var vehiculo = await ObtenerVehiculoPropioAsync(request.VehiculoId, usuarioId);

        var fecha = request.Fecha ?? DateTime.UtcNow;
        var tipoCombustibleId = request.TipoCombustibleId ?? vehiculo.TipoCombustibleId;

        await ValidarCombustibleYEstacionAsync(request.TipoCombustibleId, tipoCombustibleId, request.EstacionServicioId);

        var cargaAnterior = await ObtenerCargaAnteriorAsync(vehiculo.Id, fecha, idAExcluir: null);
        var cargaPosterior = await ObtenerCargaPosteriorAsync(vehiculo.Id, fecha, idAExcluir: null);

        ValidarSecuenciaKilometraje(request.Kilometraje, cargaAnterior, cargaPosterior);

        var kilometrosRecorridos = request.KilometrosRecorridos
            ?? (cargaAnterior is null ? null : request.Kilometraje - cargaAnterior.Kilometraje);

        var carga = new Carga
        {
            VehiculoId = vehiculo.Id,
            TipoCombustibleId = tipoCombustibleId,
            EstacionServicioId = request.EstacionServicioId,
            Fecha = fecha,
            Kilometraje = request.Kilometraje,
            KilometrosRecorridos = kilometrosRecorridos,
            Galones = request.Galones,
            CostoTotal = request.CostoTotal,
            Estado = true,
            UsuarioCreacion = usuarioId,
            FechaCreacion = DateTime.UtcNow
        };

        _dbContext.Cargas.Add(carga);
        await _dbContext.SaveChangesAsync();

        return await ObtenerRespuestaAsync(carga.Id);
    }

    public async Task<CargaResponse> ActualizarAsync(int id, CrearCargaRequest request, int usuarioId)
    {
        var carga = await ObtenerCargaPropiaAsync(id, usuarioId);
        var vehiculo = await ObtenerVehiculoPropioAsync(request.VehiculoId, usuarioId);

        var fecha = request.Fecha ?? carga.Fecha;
        var tipoCombustibleId = request.TipoCombustibleId ?? vehiculo.TipoCombustibleId;

        await ValidarCombustibleYEstacionAsync(request.TipoCombustibleId, tipoCombustibleId, request.EstacionServicioId);

        var cargaAnterior = await ObtenerCargaAnteriorAsync(vehiculo.Id, fecha, idAExcluir: carga.Id);
        var cargaPosterior = await ObtenerCargaPosteriorAsync(vehiculo.Id, fecha, idAExcluir: carga.Id);

        ValidarSecuenciaKilometraje(request.Kilometraje, cargaAnterior, cargaPosterior);

        carga.VehiculoId = vehiculo.Id;
        carga.TipoCombustibleId = tipoCombustibleId;
        carga.EstacionServicioId = request.EstacionServicioId;
        carga.Fecha = fecha;
        carga.Kilometraje = request.Kilometraje;
        carga.KilometrosRecorridos = request.KilometrosRecorridos
            ?? (cargaAnterior is null ? null : request.Kilometraje - cargaAnterior.Kilometraje);
        carga.Galones = request.Galones;
        carga.CostoTotal = request.CostoTotal;
        carga.UsuarioModificacion = usuarioId;
        carga.FechaModificacion = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return await ObtenerRespuestaAsync(carga.Id);
    }

    private async Task ValidarCombustibleYEstacionAsync(int? tipoCombustibleIdSolicitado, int tipoCombustibleId, int? estacionServicioId)
    {
        if (tipoCombustibleIdSolicitado is not null)
        {
            var tipoCombustibleExiste = await _dbContext.TiposCombustible
                .AnyAsync(t => t.Id == tipoCombustibleId && t.Estado);

            if (!tipoCombustibleExiste)
            {
                throw new ValidacionException($"No se encontró el tipo de combustible con id {tipoCombustibleId}.");
            }
        }

        if (estacionServicioId is not null)
        {
            var estacionExiste = await _dbContext.EstacionesServicio
                .AnyAsync(e => e.Id == estacionServicioId && e.Estado);

            if (!estacionExiste)
            {
                throw new ValidacionException($"No se encontró la estación de servicio con id {estacionServicioId}.");
            }
        }
    }

    private static void ValidarSecuenciaKilometraje(decimal kilometraje, Carga? cargaAnterior, Carga? cargaPosterior)
    {
        if (cargaAnterior is not null && kilometraje < cargaAnterior.Kilometraje)
        {
            throw new ValidacionException(
                $"El kilometraje ({kilometraje}) no puede ser menor al de la carga anterior ({cargaAnterior.Kilometraje}).");
        }

        if (cargaPosterior is not null && kilometraje > cargaPosterior.Kilometraje)
        {
            throw new ValidacionException(
                $"El kilometraje ({kilometraje}) no puede ser mayor al de una carga posterior ya registrada ({cargaPosterior.Kilometraje}).");
        }
    }

    public async Task EliminarAsync(int id, int usuarioId)
    {
        var carga = await ObtenerCargaPropiaAsync(id, usuarioId);

        carga.Estado = false;
        carga.FechaEliminacion = DateTime.UtcNow;
        carga.UsuarioModificacion = usuarioId;
        carga.FechaModificacion = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    private async Task<Vehiculo> ObtenerVehiculoPropioAsync(int vehiculoId, int usuarioId)
    {
        var vehiculo = await _dbContext.Vehiculos
            .FirstOrDefaultAsync(v => v.Id == vehiculoId && v.Estado && v.UsuarioPropietarioId == usuarioId);

        if (vehiculo is null)
        {
            throw new NoEncontradoException($"No se encontró el vehículo con id {vehiculoId}.");
        }

        return vehiculo;
    }

    private async Task<Carga> ObtenerCargaPropiaAsync(int id, int usuarioId)
    {
        var carga = await _dbContext.Cargas
            .FirstOrDefaultAsync(c => c.Id == id && c.Estado && c.Vehiculo!.UsuarioPropietarioId == usuarioId);

        if (carga is null)
        {
            throw new NoEncontradoException($"No se encontró la carga con id {id}.");
        }

        return carga;
    }

    // Desempata por Id cuando dos cargas caen en la misma Fecha (ej. el formulario solo
    // envía fecha sin hora, así que varias cargas del mismo día quedan a las 00:00:00Z).
    private async Task<Carga?> ObtenerCargaAnteriorAsync(int vehiculoId, DateTime fecha, int? idAExcluir) =>
        await _dbContext.Cargas
            .Where(c => c.Estado && c.VehiculoId == vehiculoId && c.Id != idAExcluir &&
                (c.Fecha < fecha || (c.Fecha == fecha && c.Id < (idAExcluir ?? int.MaxValue))))
            .OrderByDescending(c => c.Fecha)
            .ThenByDescending(c => c.Id)
            .FirstOrDefaultAsync();

    private async Task<Carga?> ObtenerCargaPosteriorAsync(int vehiculoId, DateTime fecha, int? idAExcluir) =>
        await _dbContext.Cargas
            .Where(c => c.Estado && c.VehiculoId == vehiculoId && c.Id != idAExcluir &&
                (c.Fecha > fecha || (c.Fecha == fecha && c.Id > (idAExcluir ?? 0))))
            .OrderBy(c => c.Fecha)
            .ThenBy(c => c.Id)
            .FirstOrDefaultAsync();

    private async Task<CargaResponse> ObtenerRespuestaAsync(int id) =>
        await _dbContext.Cargas
            .Where(c => c.Id == id)
            .Select(c => ProyectarRespuesta(c))
            .FirstAsync();

    private static CargaResponse ProyectarRespuesta(Carga c) => new()
    {
        Id = c.Id,
        VehiculoId = c.VehiculoId,
        VehiculoNombre = c.Vehiculo!.Nombre,
        TipoCombustibleId = c.TipoCombustibleId,
        TipoCombustibleNombre = c.TipoCombustible!.Nombre,
        EstacionServicioId = c.EstacionServicioId,
        EstacionServicioNombre = c.EstacionServicio != null ? c.EstacionServicio.Nombre : null,
        Fecha = c.Fecha,
        Kilometraje = c.Kilometraje,
        KilometrosRecorridos = c.KilometrosRecorridos,
        Galones = c.Galones,
        CostoTotal = c.CostoTotal,
        PrecioPorGalon = c.PrecioPorGalon,
        Litros = c.Litros,
        PrecioPorLitro = c.PrecioPorLitro
    };
}
