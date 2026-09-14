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
    // Hash bcrypt válido de una contraseña fija que nadie usa. Cuando el email no existe,
    // se verifica contra este hash en vez de saltarse BCrypt.Verify por completo: así el
    // tiempo de respuesta de un login con email inexistente no se distingue del de un
    // email real con contraseña incorrecta, cerrando un canal de timing para enumerar
    // qué emails están registrados.
    private const string HashDummyParaTiempoConstante =
        "$2a$11$LOzCWkymN2UuYvr/wSxxSOUrWoU5GZ58mGYy5lLh723TETFLV5x6O";

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

        var hashContraVerificar = usuario?.PasswordHash ?? HashDummyParaTiempoConstante;

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Password, hashContraVerificar))
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
