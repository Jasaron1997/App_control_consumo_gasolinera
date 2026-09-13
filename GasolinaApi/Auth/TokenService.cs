using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GasolinaApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GasolinaApi.Auth;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario)
    {
        var expiraEn = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiracionMinutos);

        var claims = new[]
        {
            new Claim(ClaimsGasolina.UsuarioId, usuario.Id.ToString()),
            new Claim(ClaimsGasolina.TokenVersion, usuario.TokenVersion.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var clavePrivada = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credenciales = new SigningCredentials(clavePrivada, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiraEn,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEn);
    }
}
