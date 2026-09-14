using System.Net;
using System.Text.Json;
using GasolinaApi.DTOs;
using GasolinaApi.Exceptions;

namespace GasolinaApi.Middleware;

public class ManejoErroresMiddleware
{
    private static readonly JsonSerializerOptions OpcionesSerializacion = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ManejoErroresMiddleware> _logger;

    public ManejoErroresMiddleware(RequestDelegate siguiente, ILogger<ManejoErroresMiddleware> logger)
    {
        _siguiente = siguiente;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (Exception excepcion)
        {
            await ManejarExcepcionAsync(contexto, excepcion);
        }
    }

    private async Task ManejarExcepcionAsync(HttpContext contexto, Exception excepcion)
    {
        var (codigoHttp, mensaje) = excepcion switch
        {
            NoEncontradoException => (HttpStatusCode.NotFound, excepcion.Message),
            ValidacionException => (HttpStatusCode.BadRequest, excepcion.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, excepcion.Message),
            _ => (HttpStatusCode.InternalServerError, "Ocurrió un error inesperado. Intenta de nuevo más tarde.")
        };

        if (codigoHttp == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(excepcion, "Error no controlado procesando {Metodo} {Ruta}", contexto.Request.Method, contexto.Request.Path);
        }

        var respuesta = RespuestaApi<object>.Error(mensaje);

        contexto.Response.ContentType = "application/json";
        contexto.Response.StatusCode = (int)codigoHttp;

        await contexto.Response.WriteAsync(JsonSerializer.Serialize(respuesta, OpcionesSerializacion));
    }
}
