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

    /// <summary>Como ObtenerUsuarioId, pero sin lanzar: para código de auditoría que debe
    /// funcionar igual con o sin usuario autenticado (ej. un intento de login fallido).</summary>
    public static int? ObtenerUsuarioIdOpcional(this ClaimsPrincipal usuario)
    {
        if (usuario.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var valor = usuario.FindFirstValue(ClaimsGasolina.UsuarioId);
        return int.TryParse(valor, out var usuarioId) ? usuarioId : null;
    }
}
