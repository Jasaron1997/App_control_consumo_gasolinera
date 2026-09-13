using System.Security.Claims;
using GasolinaApi.Auth;
using GasolinaApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace GasolinaApi.Filters;

/// <summary>
/// Invalida sesiones viejas: compara el TokenVersion del JWT contra el valor actual
/// en TB_USUARIO. Si no coincide (ej. el usuario cambió su password), responde 401.
/// </summary>
public class ValidarTokenVersionFilter : IAsyncAuthorizationFilter
{
    private readonly AppDbContext _dbContext;

    public ValidarTokenVersionFilter(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext contexto)
    {
        var endpoint = contexto.HttpContext.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() is not null)
        {
            return;
        }

        if (contexto.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var usuarioId = contexto.HttpContext.User.ObtenerUsuarioId();
        var tokenVersionClaim = contexto.HttpContext.User.FindFirstValue(ClaimsGasolina.TokenVersion);

        if (tokenVersionClaim is null || !int.TryParse(tokenVersionClaim, out var tokenVersion))
        {
            contexto.Result = new UnauthorizedResult();
            return;
        }

        var tokenVersionActual = await _dbContext.Usuarios
            .Where(u => u.Id == usuarioId)
            .Select(u => (int?)u.TokenVersion)
            .FirstOrDefaultAsync();

        if (tokenVersionActual is null || tokenVersionActual.Value != tokenVersion)
        {
            contexto.Result = new UnauthorizedResult();
        }
    }
}
