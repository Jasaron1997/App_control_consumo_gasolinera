using System.Text.Json;
using System.Text.Json.Nodes;
using GasolinaApi.Auth;
using GasolinaApi.Data;
using GasolinaApi.Models;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GasolinaApi.Filters;

public class AuditoriaActionFilter : IAsyncActionFilter
{
    // Redacta por coincidencia parcial (no lista exacta) para que cubra automáticamente
    // cualquier campo futuro cuyo nombre contenga "password" (ej. PasswordActual,
    // PasswordNuevo, PasswordHash), sin depender de mantener actualizada una lista fija.
    private static readonly string[] PalabrasClaveSensibles = { "password" };

    private readonly AppDbContext _dbContext;
    private readonly ILogger<AuditoriaActionFilter> _logger;

    public AuditoriaActionFilter(AppDbContext dbContext, ILogger<AuditoriaActionFilter> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor descriptor ||
            context.ActionDescriptor.EndpointMetadata.Any(m => m is SinAuditoriaAttribute))
        {
            await next();
            return;
        }

        var resultContext = await next();

        try
        {
            await RegistrarAsync(context, resultContext, descriptor);
        }
        catch (Exception excepcionAuditoria)
        {
            _logger.LogError(excepcionAuditoria, "No se pudo registrar la auditoría de {Controlador}.{Accion}",
                descriptor.ControllerName, descriptor.ActionName);
        }
    }

    private async Task RegistrarAsync(ActionExecutingContext context, ActionExecutedContext resultContext,
        ControllerActionDescriptor descriptor)
    {
        var usuarioId = ObtenerUsuarioIdOpcional(context.HttpContext);
        var vistaOrigen = context.HttpContext.Request.Headers["X-Vista-Origen"].FirstOrDefault();
        var tipoAccion = InferirTipoAccion(context.HttpContext.Request.Method);

        var log = new LogAuditoria
        {
            UsuarioId = usuarioId,
            Fecha = DateTime.UtcNow,
            TipoAccion = tipoAccion,
            Entidad = descriptor.ControllerName,
            IdRegistro = ObtenerIdRegistro(context, resultContext),
            Parametros = SerializarParametros(context.ActionArguments),
            Controlador = descriptor.ControllerName,
            AccionMetodo = descriptor.ActionName,
            VistaOrigen = vistaOrigen,
            Exitoso = resultContext.Exception is null,
            MensajeError = Truncar(resultContext.Exception?.Message, 500)
        };

        _dbContext.LogsAuditoria.Add(log);
        await _dbContext.SaveChangesAsync();
    }

    private static int? ObtenerUsuarioIdOpcional(HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var valor = httpContext.User.FindFirst(ClaimsGasolina.UsuarioId)?.Value;
        return int.TryParse(valor, out var usuarioId) ? usuarioId : null;
    }

    private static string? Truncar(string? valor, int longitudMaxima) =>
        valor is null || valor.Length <= longitudMaxima ? valor : valor[..longitudMaxima];

    private static string InferirTipoAccion(string metodoHttp) => metodoHttp.ToUpperInvariant() switch
    {
        "POST" => "INSERT",
        "PUT" or "PATCH" => "UPDATE",
        "DELETE" => "DELETE",
        _ => "SELECT"
    };

    private static string? ObtenerIdRegistro(ActionExecutingContext context, ActionExecutedContext resultContext)
    {
        if (context.ActionArguments.TryGetValue("id", out var idDeRuta) && idDeRuta is not null)
        {
            return idDeRuta.ToString();
        }

        if (resultContext.Result is Microsoft.AspNetCore.Mvc.ObjectResult objectResult)
        {
            var idGenerado = objectResult.Value?.GetType().GetProperty("Datos")?.GetValue(objectResult.Value);
            var id = idGenerado?.GetType().GetProperty("Id")?.GetValue(idGenerado);
            return id?.ToString();
        }

        return null;
    }

    private static string? SerializarParametros(IDictionary<string, object?> argumentos)
    {
        if (argumentos.Count == 0)
        {
            return null;
        }

        var opciones = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };

        var nodo = new JsonObject();
        foreach (var (clave, valor) in argumentos)
        {
            try
            {
                nodo[clave] = valor is null ? null : JsonSerializer.SerializeToNode(valor, valor.GetType(), opciones);
            }
            catch
            {
                nodo[clave] = "<no serializable>";
            }
        }

        RedactarCamposSensibles(nodo);

        return nodo.ToJsonString();
    }

    private static void RedactarCamposSensibles(JsonNode? nodo)
    {
        switch (nodo)
        {
            case JsonObject objeto:
                foreach (var propiedad in objeto.ToList())
                {
                    if (EsCampoSensible(propiedad.Key))
                    {
                        objeto[propiedad.Key] = "***";
                    }
                    else
                    {
                        RedactarCamposSensibles(propiedad.Value);
                    }
                }
                break;
            case JsonArray arreglo:
                foreach (var elemento in arreglo)
                {
                    RedactarCamposSensibles(elemento);
                }
                break;
        }
    }

    private static bool EsCampoSensible(string nombrePropiedad) =>
        PalabrasClaveSensibles.Any(palabra => nombrePropiedad.Contains(palabra, StringComparison.OrdinalIgnoreCase));
}
