using GasolinaApi.Data;
using GasolinaApi.DTOs.Responses;
using GasolinaApi.Models;
using GasolinaApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GasolinaApi.Services;

public class EstadisticaService : IEstadisticaService
{
    private readonly AppDbContext _dbContext;

    public EstadisticaService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ResumenEstadisticasResponse> ObtenerResumenAsync(int usuarioId, int? vehiculoId)
    {
        var cargas = ObtenerCargasDelUsuario(usuarioId, vehiculoId);

        var cargasConRecorrido = cargas.Where(c => c.KilometrosRecorridos != null && c.Galones != 0);

        var rendimientoPromedio = await cargasConRecorrido
            .Select(c => c.KilometrosRecorridos!.Value / c.Galones)
            .AverageAsyncSiHayDatos();

        var sumaCostoTotal = await cargasConRecorrido.SumAsync(c => (decimal?)c.CostoTotal) ?? 0m;
        var sumaKilometros = await cargasConRecorrido.SumAsync(c => (decimal?)c.KilometrosRecorridos) ?? 0m;
        var costoPorKm = sumaKilometros == 0 ? (decimal?)null : sumaCostoTotal / sumaKilometros;

        var (inicioMes, inicioMesSiguiente) = ObtenerRangoMesActual();
        var cargasDelMes = cargas.Where(c => c.Fecha >= inicioMes && c.Fecha < inicioMesSiguiente);

        var gastoMes = await cargasDelMes.SumAsync(c => (decimal?)c.CostoTotal) ?? 0m;
        var kmRecorridosMes = await cargasDelMes.SumAsync(c => (decimal?)c.KilometrosRecorridos) ?? 0m;

        return new ResumenEstadisticasResponse
        {
            RendimientoPromedio = rendimientoPromedio,
            GastoMes = gastoMes,
            CostoPorKm = costoPorKm,
            KmRecorridosMes = kmRecorridosMes
        };
    }

    public async Task<List<HistoricoPuntoResponse>> ObtenerHistoricoAsync(int usuarioId, int? vehiculoId, int meses)
    {
        var desde = DateTime.UtcNow.AddMonths(-meses);

        return await ObtenerCargasDelUsuario(usuarioId, vehiculoId)
            .Where(c => c.Fecha >= desde)
            .OrderBy(c => c.Fecha)
            .Select(c => new HistoricoPuntoResponse
            {
                Fecha = c.Fecha,
                Rendimiento = c.KilometrosRecorridos != null && c.Galones != 0
                    ? c.KilometrosRecorridos.Value / c.Galones
                    : (decimal?)null,
                CostoTotal = c.CostoTotal
            })
            .ToListAsync();
    }

    private IQueryable<Carga> ObtenerCargasDelUsuario(int usuarioId, int? vehiculoId)
    {
        var consulta = _dbContext.Cargas.Where(c => c.Estado && c.Vehiculo!.UsuarioPropietarioId == usuarioId);

        if (vehiculoId is not null)
        {
            consulta = consulta.Where(c => c.VehiculoId == vehiculoId);
        }

        return consulta;
    }

    private static (DateTime Inicio, DateTime InicioSiguiente) ObtenerRangoMesActual()
    {
        var ahora = DateTime.UtcNow;
        var inicioMes = new DateTime(ahora.Year, ahora.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var inicioMesSiguiente = inicioMes.AddMonths(1);
        return (inicioMes, inicioMesSiguiente);
    }
}

internal static class QueryableExtensions
{
    public static async Task<decimal?> AverageAsyncSiHayDatos(this IQueryable<decimal> consulta)
    {
        var hayDatos = await consulta.AnyAsync();
        return hayDatos ? await consulta.AverageAsync() : null;
    }
}
