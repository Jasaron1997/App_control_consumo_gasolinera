using GasolinaApi.DTOs.Requests;
using GasolinaApi.DTOs.Responses;

namespace GasolinaApi.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task CambiarPasswordAsync(int usuarioId, CambiarPasswordRequest request);
}
