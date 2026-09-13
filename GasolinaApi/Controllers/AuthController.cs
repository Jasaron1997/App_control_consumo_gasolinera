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
}
