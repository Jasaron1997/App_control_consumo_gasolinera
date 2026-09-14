using System.Linq.Expressions;
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
            .Select(ProyeccionRespuesta)
            .ToListAsync();

    public async Task<VehiculoResponse> CrearAsync(VehiculoRequest request, int usuarioId)
    {
        await ValidarCatalogosAsync(request);
        await ValidarPlacaDisponibleAsync(request.Placa, idAExcluir: null);

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
        await GuardarValidandoPlacaAsync(request.Placa);

        return await ObtenerRespuestaAsync(vehiculo.Id);
    }

    public async Task<VehiculoResponse> ActualizarAsync(int id, VehiculoRequest request, int usuarioId)
    {
        var vehiculo = await ObtenerPropioAsync(id, usuarioId);

        await ValidarCatalogosAsync(request);
        await ValidarPlacaDisponibleAsync(request.Placa, idAExcluir: vehiculo.Id);

        vehiculo.TipoVehiculoId = request.TipoVehiculoId;
        vehiculo.TipoCombustibleId = request.TipoCombustibleId;
        vehiculo.Nombre = request.Nombre;
        vehiculo.Placa = request.Placa;
        vehiculo.TanqueCapacidadGalones = request.TanqueCapacidadGalones;
        vehiculo.UsuarioModificacion = usuarioId;
        vehiculo.FechaModificacion = DateTime.UtcNow;

        await GuardarValidandoPlacaAsync(request.Placa);

        return await ObtenerRespuestaAsync(vehiculo.Id);
    }

    // ValidarPlacaDisponibleAsync ya revisó la placa antes de llegar aquí, pero esa
    // comprobación y este SaveChangesAsync son dos round-trips separados sin transacción:
    // dos requests concurrentes con la misma placa pueden pasar ambos la validación antes
    // de que cualquiera guarde. El índice único filtrado (UQ_VEHICULO_PLACA_ACTIVA) sigue
    // siendo la garantía real contra ese caso; esto solo traduce su violación al mismo 400
    // amigable que ya produce la validación previa, en vez de un 500 genérico.
    private async Task GuardarValidandoPlacaAsync(string? placa)
    {
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException excepcion) when (excepcion.InnerException?.Message
            .Contains("UQ_VEHICULO_PLACA_ACTIVA", StringComparison.OrdinalIgnoreCase) == true)
        {
            // No se echa la placa de vuelta: el índice único es global (no por usuario,
            // ver 01_DDL_Tablas.sql), así que confirmar el valor exacto en el mensaje le
            // daría a cualquier usuario autenticado un oráculo para probar si una placa
            // ajena está registrada por otro usuario.
            throw new ValidacionException("Ya existe un vehículo activo con esa placa.");
        }
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

    private async Task ValidarPlacaDisponibleAsync(string? placa, int? idAExcluir)
    {
        if (string.IsNullOrWhiteSpace(placa))
        {
            return;
        }

        var placaEnUso = await _dbContext.Vehiculos
            .AnyAsync(v => v.Estado && v.Placa == placa && v.Id != idAExcluir);

        if (placaEnUso)
        {
            // Mismo motivo que en GuardarValidandoPlacaAsync: no confirmar el valor exacto.
            throw new ValidacionException("Ya existe un vehículo activo con esa placa.");
        }
    }

    private async Task<VehiculoResponse> ObtenerRespuestaAsync(int id) =>
        await _dbContext.Vehiculos
            .Where(v => v.Id == id)
            .Select(ProyeccionRespuesta)
            .FirstAsync();

    // Debe ser una Expression<Func<>> (no un método normal): así EF Core la traduce
    // a SQL con los JOIN necesarios. Si esto fuera una llamada a método, EF Core no
    // podría traducirla, traería el Vehiculo sin sus relaciones, y "v.TipoVehiculo!.Nombre"
    // reventaría en tiempo de ejecución con NullReferenceException en cada request.
    private static readonly Expression<Func<Vehiculo, VehiculoResponse>> ProyeccionRespuesta = v => new VehiculoResponse
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
