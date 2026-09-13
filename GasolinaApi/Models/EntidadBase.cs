namespace GasolinaApi.Models;

public abstract class EntidadBase
{
    public int Id { get; set; }
    public int? UsuarioCreacion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? UsuarioModificacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public bool Estado { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}
