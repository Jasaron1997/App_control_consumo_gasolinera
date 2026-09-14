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
        // Se materializa una sola vez y el resto se calcula en memoria, en vez de
        // seis round-trips separados a SQL Server para los distintos agregados.
        var cargas = await ObtenerCargasDelUsuario(usuarioId, vehiculoId)
            .Select(c => new { c.Fecha, c.KilometrosRecorridos, c.Galones, c.CostoTotal })
            .ToListAsync();

        var cargasConRecorrido = cargas.Where(c => c.KilometrosRecorridos != null && c.Galones != 0).ToList();

        var sumaCostoTotal = cargasConRecorrido.Sum(c => c.CostoTotal);
        var sumaKilometros = cargasConRecorrido.Sum(c => c.KilometrosRecorridos!.Value);
        var sumaGalones = cargasConRecorrido.Sum(c => c.Galones);

        // Promedio ponderado (suma de km / suma de galones), no promedio de razones:
        // el promedio de razones le da el mismo peso a una carga de 4km que a una de
        // 400km, distorsionando el rendimiento real cuando el tamaño de las cargas
        // varía. Es el mismo criterio que ya usa CostoPorKm un poco más abajo.
        var rendimientoPromedio = sumaGalones == 0 ? (decimal?)null : sumaKilometros / sumaGalones;
        var costoPorKm = sumaKilometros == 0 ? (decimal?)null : sumaCostoTotal / sumaKilometros;

        var (inicioMes, inicioMesSiguiente) = ObtenerRangoMesActual();
        var cargasDelMes = cargas.Where(c => c.Fecha >= inicioMes && c.Fecha < inicioMesSiguiente).ToList();

        var gastoMes = cargasDelMes.Sum(c => c.CostoTotal);
        var kmRecorridosMes = cargasDelMes.Sum(c => c.KilometrosRecorridos ?? 0);

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
        // Fecha se guarda como fecha-calendario local sin offset (Kind Unspecified, ej.
        // "2026-08-31T00:00:00"), igual que en FormularioCarga.fechaLocalHoy(). Comparar
        // contra un corte basado en UtcNow desalinea el rango en las últimas horas de
        // cada día en husos horarios negativos (ej. Guatemala), así que se usa DateTime.Now.
        // Se trunca a medianoche (.Date) para no excluir, según la hora del día en que se
        // consulta, la propia carga del día límite guardada a las 00:00:00.
        var desde = DateTime.Now.Date.AddMonths(-meses);

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

    // Mismo motivo que en ObtenerHistoricoAsync: Fecha es una fecha-calendario local sin
    // offset, así que el mes "actual" se calcula con DateTime.Now, no UtcNow.
    private static (DateTime Inicio, DateTime InicioSiguiente) ObtenerRangoMesActual()
    {
        var ahora = DateTime.Now;
        var inicioMes = new DateTime(ahora.Year, ahora.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var inicioMesSiguiente = inicioMes.AddMonths(1);
        return (inicioMes, inicioMesSiguiente);
    }
}
