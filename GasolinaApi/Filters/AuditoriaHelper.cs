using Microsoft.Extensions.Logging;

namespace GasolinaApi.Filters;

/// <summary>
/// Piezas compartidas entre AuditoriaActionFilter (cubre el camino normal de cada
/// request) y ValidacionModeloResponseFactory (cubre el 400 automático de
/// [ApiController], que corre antes de que cualquier ActionFilter lo intercepte).
/// </summary>
internal static class AuditoriaHelper
{
    public const int LongitudMaximaMensajeError = 500;

    // Coincidencia parcial (no lista exacta) para que cubra automáticamente cualquier
    // campo cuyo nombre contenga "password" (ej. Password, PasswordHash, PasswordActual,
    // PasswordNuevo), sin depender de mantener actualizada una lista fija.
    private static readonly string[] PalabrasClaveSensibles = { "password" };

    public static string InferirTipoAccion(string metodoHttp) => metodoHttp.ToUpperInvariant() switch
    {
        "POST" => "INSERT",
        "PUT" or "PATCH" => "UPDATE",
        "DELETE" => "DELETE",
        _ => "SELECT"
    };

    public static string? Truncar(string? valor, int longitudMaxima = LongitudMaximaMensajeError) =>
        valor is null || valor.Length <= longitudMaxima ? valor : valor[..longitudMaxima];

    public static bool EsCampoSensible(string nombrePropiedad) =>
        PalabrasClaveSensibles.Any(palabra => nombrePropiedad.Contains(palabra, StringComparison.OrdinalIgnoreCase));

    // Tercer punto que necesita "no dejar que un fallo al auditar tumbe la respuesta al
    // cliente, pero tampoco quedar mudo": junto con AuditoriaActionFilter y
    // ValidacionModeloResponseFactory, se centraliza aquí para no repetir el mismo
    // catch+log con mensajes ligeramente distintos en cada uno.
    public static void RegistrarFalloDeAuditoria(ILogger logger, Exception excepcion, string contexto) =>
        logger.LogError(excepcion, "No se pudo registrar en auditoría: {Contexto}.", contexto);
}
