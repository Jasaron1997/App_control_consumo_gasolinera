using GasolinaApi.Data;
using GasolinaApi.DTOs;
using GasolinaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace GasolinaApi.Filters;

/// <summary>
/// [ApiController] devuelve automáticamente 400 cuando el ModelState es inválido (ej. un
/// [Range]/[Required] fallido), y lo hace ANTES de que corra cualquier ActionFilter —
/// incluido AuditoriaActionFilter — dejando un hueco en TB_LOG_AUDITORIA para todo
/// request rechazado por validación. Esta fábrica reemplaza esa respuesta automática
/// para que use el mismo sobre RespuestaApi&lt;T&gt; que el resto de la API y deje su
/// propio registro de auditoría.
/// </summary>
public static class ValidacionModeloResponseFactory
{
    public static IActionResult Crear(ActionContext contexto)
    {
        var mensaje = string.Join(" ", contexto.ModelState
            .Where(par => par.Value?.Errors.Count > 0)
            .SelectMany(par => par.Value!.Errors.Select(error => error.ErrorMessage)));

        if (string.IsNullOrWhiteSpace(mensaje))
        {
            mensaje = "La solicitud contiene datos inválidos.";
        }

        RegistrarAuditoria(contexto, mensaje);

        return new BadRequestObjectResult(RespuestaApi<object>.Error(mensaje));
    }

    private static void RegistrarAuditoria(ActionContext contexto, string mensaje)
    {
        try
        {
            var dbContext = contexto.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var descriptor = contexto.ActionDescriptor as ControllerActionDescriptor;
            var usuarioIdClaim = contexto.HttpContext.User.FindFirst(Auth.ClaimsGasolina.UsuarioId)?.Value;
            var mensajeTruncado = mensaje.Length > 500 ? mensaje[..500] : mensaje;

            dbContext.LogsAuditoria.Add(new LogAuditoria
            {
                UsuarioId = int.TryParse(usuarioIdClaim, out var usuarioId) ? usuarioId : null,
                Fecha = DateTime.UtcNow,
                TipoAccion = InferirTipoAccion(contexto.HttpContext.Request.Method),
                Entidad = descriptor?.ControllerName ?? "Desconocido",
                Controlador = descriptor?.ControllerName,
                AccionMetodo = descriptor?.ActionName,
                VistaOrigen = contexto.HttpContext.Request.Headers["X-Vista-Origen"].FirstOrDefault(),
                Exitoso = false,
                MensajeError = mensajeTruncado
            });

            dbContext.SaveChanges();
        }
        catch
        {
            // No dejar que una falla de auditoría bloquee la respuesta de validación al cliente.
        }
    }

    private static string InferirTipoAccion(string metodoHttp) => metodoHttp.ToUpperInvariant() switch
    {
        "POST" => "INSERT",
        "PUT" or "PATCH" => "UPDATE",
        "DELETE" => "DELETE",
        _ => "SELECT"
    };
}
