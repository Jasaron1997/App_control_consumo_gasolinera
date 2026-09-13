using System.Text.Json.Nodes;
using GasolinaApi.Auth;
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
/// propio registro de auditoría (ver AuditoriaHelper para lo compartido con
/// AuditoriaActionFilter).
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

            dbContext.LogsAuditoria.Add(new LogAuditoria
            {
                UsuarioId = contexto.HttpContext.User.ObtenerUsuarioIdOpcional(),
                Fecha = DateTime.UtcNow,
                TipoAccion = AuditoriaHelper.InferirTipoAccion(contexto.HttpContext.Request.Method),
                Entidad = descriptor?.ControllerName ?? "Desconocido",
                Parametros = SerializarValoresEnviados(contexto),
                Controlador = descriptor?.ControllerName,
                AccionMetodo = descriptor?.ActionName,
                VistaOrigen = contexto.HttpContext.Request.Headers["X-Vista-Origen"].FirstOrDefault(),
                Exitoso = false,
                MensajeError = AuditoriaHelper.Truncar(mensaje)
            });

            dbContext.SaveChanges();
        }
        catch
        {
            // No dejar que una falla de auditoría bloquee la respuesta de validación al cliente.
        }
    }

    // El modelo no llegó a bindearse en un DTO utilizable (por eso falló la validación),
    // así que se arma el registro a partir de lo que el ModelState sí capturó: el valor
    // que el cliente intentó mandar en cada campo con error.
    private static string? SerializarValoresEnviados(ActionContext contexto)
    {
        var nodo = new JsonObject();

        foreach (var (clave, estado) in contexto.ModelState)
        {
            if (estado is null || estado.Errors.Count == 0)
            {
                continue;
            }

            var valor = estado.AttemptedValue;
            nodo[clave] = AuditoriaHelper.EsCampoSensible(clave) ? "***" : (JsonNode?)valor;
        }

        return nodo.Count == 0 ? null : nodo.ToJsonString();
    }
}
