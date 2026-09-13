using GasolinaApi.DTOs.Requests;
using GasolinaApi.DTOs.Responses;

namespace GasolinaApi.Services.Interfaces;

public interface ICargaService
{
    Task<List<CargaResponse>> ObtenerAsync(int usuarioId, int? vehiculoId, DateTime? desde, DateTime? hasta);
    Task<CargaResponse> CrearAsync(CrearCargaRequest request, int usuarioId);
    Task<CargaResponse> ActualizarAsync(int id, CrearCargaRequest request, int usuarioId);
    Task EliminarAsync(int id, int usuarioId);
}
