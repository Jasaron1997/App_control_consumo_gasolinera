namespace GasolinaApi.DTOs;

public class RespuestaApi<T>
{
    public bool Exito { get; set; }
    public T? Datos { get; set; }
    public string? Mensaje { get; set; }

    public static RespuestaApi<T> Ok(T datos, string? mensaje = null) => new()
    {
        Exito = true,
        Datos = datos,
        Mensaje = mensaje
    };

    public static RespuestaApi<T> Error(string mensaje) => new()
    {
        Exito = false,
        Datos = default,
        Mensaje = mensaje
    };
}
