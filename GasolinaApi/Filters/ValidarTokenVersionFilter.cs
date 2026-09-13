using System.Security.Claims;
using GasolinaApi.Auth;
using GasolinaApi.Data;
using GasolinaApi.DTOs;
using GasolinaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
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
            const string mensaje = "Token sin TokenVersion válido.";
            contexto.Result = ResultadoNoAutorizado(mensaje);
            await RegistrarRechazoAsync(contexto, usuarioId, mensaje);
            return;
        }

        var tokenVersionActual = await _dbContext.Usuarios
            .Where(u => u.Id == usuarioId)
            .Select(u => (int?)u.TokenVersion)
            .FirstOrDefaultAsync();

        if (tokenVersionActual is null || tokenVersionActual.Value != tokenVersion)
        {
            const string mensaje = "La sesión ya no es válida. Inicia sesión de nuevo.";
            contexto.Result = ResultadoNoAutorizado(mensaje);
            await RegistrarRechazoAsync(contexto, usuarioId, "TokenVersion desactualizado (sesión invalidada).");
        }
    }

    // Sin esto, el 401 sale con el ProblemDetails genérico de ASP.NET Core en vez del
    // mismo sobre RespuestaApi<T> que usa el resto de la API (incluido el 401 de
    // credenciales inválidas en el login, que si pasa por ManejoErroresMiddleware).
    private static UnauthorizedObjectResult ResultadoNoAutorizado(string mensaje) =>
        new(RespuestaApi<object>.Error(mensaje));

    // Este filtro corta la solicitud con 401 antes de que AuditoriaActionFilter (un
    // ActionFilter) llegue a ejecutarse, así que sin esto un token viejo/invalidado
    // rechazado aquí no dejaría ningún rastro en TB_LOG_AUDITORIA.
    private async Task RegistrarRechazoAsync(AuthorizationFilterContext contexto, int usuarioId, string mensaje)
    {
        try
        {
            var descriptor = contexto.ActionDescriptor as ControllerActionDescriptor;

            _dbContext.LogsAuditoria.Add(new LogAuditoria
            {
                UsuarioId = usuarioId,
                Fecha = DateTime.UtcNow,
                TipoAccion = AuditoriaHelper.InferirTipoAccion(contexto.HttpContext.Request.Method),
                Entidad = descriptor?.ControllerName ?? "Desconocido",
                Controlador = descriptor?.ControllerName,
                AccionMetodo = descriptor?.ActionName,
                VistaOrigen = contexto.HttpContext.Request.Headers["X-Vista-Origen"].FirstOrDefault(),
                Exitoso = false,
                MensajeError = AuditoriaHelper.Truncar(mensaje)
            });

            await _dbContext.SaveChangesAsync();
        }
        catch
        {
            // No dejar que una falla de auditoría bloquee la respuesta 401 al cliente.
        }
    }
}
