using GasolinaApi.Data;
using GasolinaApi.DTOs.Requests;
using GasolinaApi.DTOs.Responses;
using GasolinaApi.Exceptions;
using GasolinaApi.Models;
using GasolinaApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GasolinaApi.Services;

public class VehiculoService : IVehiculoService
{
    private readonly AppDbContext _dbContext;

    public VehiculoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<VehiculoResponse>> ObtenerDelUsuarioAsync(int usuarioId) =>
        await _dbContext.Vehiculos
            .Where(v => v.Estado && v.UsuarioPropietarioId == usuarioId)
            .OrderBy(v => v.Nombre)
            .Select(v => ProyectarRespuesta(v))
            .ToListAsync();

    public async Task<VehiculoResponse> CrearAsync(VehiculoRequest request, int usuarioId)
    {
        await ValidarCatalogosAsync(request);

        var vehiculo = new Vehiculo
        {
            TipoVehiculoId = request.TipoVehiculoId,
            TipoCombustibleId = request.TipoCombustibleId,
            UsuarioPropietarioId = usuarioId,
            Nombre = request.Nombre,
            Placa = request.Placa,
            TanqueCapacidadGalones = request.TanqueCapacidadGalones,
            Estado = true,
            UsuarioCreacion = usuarioId,
            FechaCreacion = DateTime.UtcNow
        };

        _dbContext.Vehiculos.Add(vehiculo);
        await _dbContext.SaveChangesAsync();

        return await ObtenerRespuestaAsync(vehiculo.Id);
    }

    public async Task<VehiculoResponse> ActualizarAsync(int id, VehiculoRequest request, int usuarioId)
    {
        var vehiculo = await ObtenerPropioAsync(id, usuarioId);

        await ValidarCatalogosAsync(request);

        vehiculo.TipoVehiculoId = request.TipoVehiculoId;
        vehiculo.TipoCombustibleId = request.TipoCombustibleId;
        vehiculo.Nombre = request.Nombre;
        vehiculo.Placa = request.Placa;
        vehiculo.TanqueCapacidadGalones = request.TanqueCapacidadGalones;
        vehiculo.UsuarioModificacion = usuarioId;
        vehiculo.FechaModificacion = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return await ObtenerRespuestaAsync(vehiculo.Id);
    }

    public async Task EliminarAsync(int id, int usuarioId)
    {
        var vehiculo = await ObtenerPropioAsync(id, usuarioId);

        vehiculo.Estado = false;
        vehiculo.FechaEliminacion = DateTime.UtcNow;
        vehiculo.UsuarioModificacion = usuarioId;
        vehiculo.FechaModificacion = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    private async Task<Vehiculo> ObtenerPropioAsync(int id, int usuarioId)
    {
        var vehiculo = await _dbContext.Vehiculos
            .FirstOrDefaultAsync(v => v.Id == id && v.Estado && v.UsuarioPropietarioId == usuarioId);

        if (vehiculo is null)
        {
            throw new NoEncontradoException($"No se encontró el vehículo con id {id}.");
        }

        return vehiculo;
    }

    private async Task ValidarCatalogosAsync(VehiculoRequest request)
    {
        var tipoVehiculoExiste = await _dbContext.TiposVehiculo.AnyAsync(t => t.Id == request.TipoVehiculoId && t.Estado);
        if (!tipoVehiculoExiste)
        {
            throw new ValidacionException($"No se encontró el tipo de vehículo con id {request.TipoVehiculoId}.");
        }

        var tipoCombustibleExiste = await _dbContext.TiposCombustible.AnyAsync(t => t.Id == request.TipoCombustibleId && t.Estado);
        if (!tipoCombustibleExiste)
        {
            throw new ValidacionException($"No se encontró el tipo de combustible con id {request.TipoCombustibleId}.");
        }
    }

    private async Task<VehiculoResponse> ObtenerRespuestaAsync(int id) =>
        await _dbContext.Vehiculos
            .Where(v => v.Id == id)
            .Select(v => ProyectarRespuesta(v))
            .FirstAsync();

    private static VehiculoResponse ProyectarRespuesta(Vehiculo v) => new()
    {
        Id = v.Id,
        Nombre = v.Nombre,
        Placa = v.Placa,
        TanqueCapacidadGalones = v.TanqueCapacidadGalones,
        TipoVehiculoId = v.TipoVehiculoId,
        TipoVehiculoNombre = v.TipoVehiculo!.Nombre,
        TipoCombustibleId = v.TipoCombustibleId,
        TipoCombustibleNombre = v.TipoCombustible!.Nombre
    };
}
