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
    private static readonly JsonSerializerOptions OpcionesSerializacion = new()
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
    };

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
            AuditoriaHelper.RegistrarFalloDeAuditoria(_logger, excepcionAuditoria,
                $"{descriptor.ControllerName}.{descriptor.ActionName}");
        }
    }

    private async Task RegistrarAsync(ActionExecutingContext context, ActionExecutedContext resultContext,
        ControllerActionDescriptor descriptor)
    {
        // Si la acción falló después de rastrear cambios en este mismo DbContext (ej.
        // CrearAsync agregó una Carga y luego SaveChangesAsync explotó), hay que
        // descartarlos antes de guardar el log: si no, el intento de guardar la
        // auditoría podría persistir de rebote esa mutación a medio hacer.
        if (resultContext.Exception is not null)
        {
            _dbContext.ChangeTracker.Clear();
        }

        var log = new LogAuditoria
        {
            UsuarioId = context.HttpContext.User.ObtenerUsuarioIdOpcional(),
            Fecha = DateTime.UtcNow,
            TipoAccion = AuditoriaHelper.InferirTipoAccion(context.HttpContext.Request.Method),
            Entidad = descriptor.ControllerName,
            IdRegistro = ObtenerIdRegistro(context, resultContext),
            Parametros = SerializarParametros(context.ActionArguments),
            Controlador = descriptor.ControllerName,
            AccionMetodo = descriptor.ActionName,
            VistaOrigen = context.HttpContext.Request.Headers["X-Vista-Origen"].FirstOrDefault(),
            Exitoso = resultContext.Exception is null,
            MensajeError = AuditoriaHelper.Truncar(resultContext.Exception?.Message)
        };

        _dbContext.LogsAuditoria.Add(log);
        await _dbContext.SaveChangesAsync();
    }

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

        var nodo = new JsonObject();
        foreach (var (clave, valor) in argumentos)
        {
            try
            {
                nodo[clave] = valor is null ? null : JsonSerializer.SerializeToNode(valor, valor.GetType(), OpcionesSerializacion);
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
                    if (AuditoriaHelper.EsCampoSensible(propiedad.Key))
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
}
