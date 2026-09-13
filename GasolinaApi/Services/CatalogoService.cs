using GasolinaApi.Data;
using GasolinaApi.DTOs.Requests;
using GasolinaApi.DTOs.Responses;
using GasolinaApi.Exceptions;
using GasolinaApi.Models;
using GasolinaApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GasolinaApi.Services;

public class CatalogoService : ICatalogoService
{
    private readonly AppDbContext _dbContext;

    public CatalogoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CatalogoResponse>> ObtenerTiposVehiculoAsync() =>
        await _dbContext.TiposVehiculo
            .Where(t => t.Estado)
            .OrderBy(t => t.Nombre)
            .Select(t => new CatalogoResponse { Id = t.Id, Nombre = t.Nombre })
            .ToListAsync();

    public async Task<List<CatalogoResponse>> ObtenerTiposCombustibleAsync() =>
        await _dbContext.TiposCombustible
            .Where(t => t.Estado)
            .OrderBy(t => t.Nombre)
            .Select(t => new CatalogoResponse { Id = t.Id, Nombre = t.Nombre })
            .ToListAsync();

    public async Task<List<CatalogoResponse>> ObtenerDepartamentosAsync() =>
        await _dbContext.Departamentos
            .Where(d => d.Estado)
            .OrderBy(d => d.Nombre)
            .Select(d => new CatalogoResponse { Id = d.Id, Nombre = d.Nombre })
            .ToListAsync();

    public async Task<List<MunicipioResponse>> ObtenerMunicipiosPorDepartamentoAsync(int departamentoId)
    {
        var departamentoExiste = await _dbContext.Departamentos
            .AnyAsync(d => d.Id == departamentoId && d.Estado);

        if (!departamentoExiste)
        {
            throw new NoEncontradoException($"No se encontró el departamento con id {departamentoId}.");
        }

        return await _dbContext.Municipios
            .Where(m => m.Estado && m.DepartamentoId == departamentoId)
            .OrderBy(m => m.Nombre)
            .Select(m => new MunicipioResponse
            {
                Id = m.Id,
                Nombre = m.Nombre,
                DepartamentoId = m.DepartamentoId,
                DepartamentoNombre = m.Departamento!.Nombre
            })
            .ToListAsync();
    }

    public async Task<List<EstacionServicioResponse>> BuscarEstacionesServicioAsync(string? buscar)
    {
        var consulta = _dbContext.EstacionesServicio.Where(e => e.Estado);

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            consulta = consulta.Where(e => e.Nombre.Contains(buscar));
        }

        return await consulta
            .OrderBy(e => e.Nombre)
            .Select(e => new EstacionServicioResponse
            {
                Id = e.Id,
                Nombre = e.Nombre,
                Marca = e.Marca,
                MunicipioId = e.MunicipioId,
                MunicipioNombre = e.Municipio != null ? e.Municipio.Nombre : null,
                Latitud = e.Latitud,
                Longitud = e.Longitud,
                Referencia = e.Referencia
            })
            .Take(20)
            .ToListAsync();
    }

    public async Task<EstacionServicioResponse> CrearEstacionServicioAsync(EstacionServicioRequest request, int usuarioId)
    {
        if (request.MunicipioId is not null)
        {
            var municipioExiste = await _dbContext.Municipios
                .AnyAsync(m => m.Id == request.MunicipioId && m.Estado);

            if (!municipioExiste)
            {
                throw new ValidacionException($"No se encontró el municipio con id {request.MunicipioId}.");
            }
        }

        var estacion = new EstacionServicio
        {
            Nombre = request.Nombre,
            Marca = request.Marca,
            MunicipioId = request.MunicipioId,
            Latitud = request.Latitud,
            Longitud = request.Longitud,
            Referencia = request.Referencia,
            Estado = true,
            UsuarioCreacion = usuarioId,
            FechaCreacion = DateTime.UtcNow
        };

        _dbContext.EstacionesServicio.Add(estacion);
        await _dbContext.SaveChangesAsync();

        var municipioNombre = request.MunicipioId is null
            ? null
            : await _dbContext.Municipios.Where(m => m.Id == request.MunicipioId).Select(m => m.Nombre).FirstOrDefaultAsync();

        return new EstacionServicioResponse
        {
            Id = estacion.Id,
            Nombre = estacion.Nombre,
            Marca = estacion.Marca,
            MunicipioId = estacion.MunicipioId,
            MunicipioNombre = municipioNombre,
            Latitud = estacion.Latitud,
            Longitud = estacion.Longitud,
            Referencia = estacion.Referencia
        };
    }
}
