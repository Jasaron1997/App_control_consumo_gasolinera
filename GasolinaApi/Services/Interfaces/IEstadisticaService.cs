using GasolinaApi.DTOs.Responses;

namespace GasolinaApi.Services.Interfaces;

public interface IEstadisticaService
{
    Task<ResumenEstadisticasResponse> ObtenerResumenAsync(int usuarioId, int? vehiculoId);
    Task<List<HistoricoPuntoResponse>> ObtenerHistoricoAsync(int usuarioId, int? vehiculoId, int meses);
}
