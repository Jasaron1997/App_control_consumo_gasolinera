using GasolinaApi.Auth;
using GasolinaApi.DTOs;
using GasolinaApi.DTOs.Requests;
using GasolinaApi.DTOs.Responses;
using GasolinaApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GasolinaApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<RespuestaApi<LoginResponse>>> Login(LoginRequest request)
    {
        var resultado = await _authService.LoginAsync(request);
        return Ok(RespuestaApi<LoginResponse>.Ok(resultado));
    }

    [Authorize]
    [HttpPut("password")]
    public async Task<ActionResult<RespuestaApi<object>>> CambiarPassword(CambiarPasswordRequest request)
    {
        var usuarioId = User.ObtenerUsuarioId();
        await _authService.CambiarPasswordAsync(usuarioId, request);
        // TokenVersion++ invalida TODOS los tokens ya emitidos, incluido el que se usó
        // para llamar a este mismo endpoint — no hay "estas sesiones sí, esta no".
        return Ok(RespuestaApi<object>.Ok(new { },
            "Contraseña actualizada. Todas tus sesiones, incluida esta, quedaron cerradas: vuelve a iniciar sesión."));
    }
}
