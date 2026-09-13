using GasolinaApi.DTOs.Requests;
using GasolinaApi.DTOs.Responses;

namespace GasolinaApi.Services.Interfaces;

public interface IVehiculoService
{
    Task<List<VehiculoResponse>> ObtenerDelUsuarioAsync(int usuarioId);
    Task<VehiculoResponse> CrearAsync(VehiculoRequest request, int usuarioId);
    Task<VehiculoResponse> ActualizarAsync(int id, VehiculoRequest request, int usuarioId);
    Task EliminarAsync(int id, int usuarioId);
}
