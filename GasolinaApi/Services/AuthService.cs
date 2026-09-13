using GasolinaApi.Auth;
using GasolinaApi.Data;
using GasolinaApi.DTOs.Requests;
using GasolinaApi.DTOs.Responses;
using GasolinaApi.Exceptions;
using GasolinaApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GasolinaApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public AuthService(AppDbContext dbContext, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var usuario = await _dbContext.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Estado);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
        {
            throw new ValidacionException("Email o contraseña incorrectos.");
        }

        var (token, expiraEn) = _tokenService.GenerarToken(usuario);

        return new LoginResponse
        {
            Token = token,
            NombreUsuario = usuario.Nombre,
            ExpiraEn = expiraEn
        };
    }

    public async Task CambiarPasswordAsync(int usuarioId, CambiarPasswordRequest request)
    {
        var usuario = await _dbContext.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Estado);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.PasswordActual, usuario.PasswordHash))
        {
            throw new ValidacionException("La contraseña actual es incorrecta.");
        }

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.PasswordNuevo);
        // Incrementar TokenVersion invalida todos los JWT emitidos antes de este cambio.
        usuario.TokenVersion++;
        usuario.UsuarioModificacion = usuarioId;
        usuario.FechaModificacion = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }
}
