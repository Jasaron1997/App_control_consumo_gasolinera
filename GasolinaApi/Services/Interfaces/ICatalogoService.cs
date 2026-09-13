using GasolinaApi.DTOs.Requests;
using GasolinaApi.DTOs.Responses;

namespace GasolinaApi.Services.Interfaces;

public interface ICatalogoService
{
    Task<List<CatalogoResponse>> ObtenerTiposVehiculoAsync();
    Task<List<CatalogoResponse>> ObtenerTiposCombustibleAsync();
    Task<List<CatalogoResponse>> ObtenerDepartamentosAsync();
    Task<List<MunicipioResponse>> ObtenerMunicipiosPorDepartamentoAsync(int departamentoId);
    Task<List<EstacionServicioResponse>> BuscarEstacionesServicioAsync(string? buscar);
    Task<EstacionServicioResponse> CrearEstacionServicioAsync(EstacionServicioRequest request, int usuarioId);
}
