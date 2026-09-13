using System.Security.Claims;

namespace GasolinaApi.Auth;

public static class ClaimsPrincipalExtensions
{
    public static int ObtenerUsuarioId(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirstValue(ClaimsGasolina.UsuarioId);

        if (valor is null || !int.TryParse(valor, out var usuarioId))
        {
            throw new UnauthorizedAccessException("El token no contiene un usuario válido.");
        }

        return usuarioId;
    }
}
