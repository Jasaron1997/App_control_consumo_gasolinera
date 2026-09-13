namespace GasolinaApi.Models;

public class LogAuditoria
{
    public long Id { get; set; }
    public int? UsuarioId { get; set; }
    public DateTime Fecha { get; set; }
    public string TipoAccion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string? IdRegistro { get; set; }
    public string? Parametros { get; set; }
    public string? Controlador { get; set; }
    public string? AccionMetodo { get; set; }
    public string? VistaOrigen { get; set; }
    public bool Exitoso { get; set; }
    public string? MensajeError { get; set; }

    public Usuario? Usuario { get; set; }
}
