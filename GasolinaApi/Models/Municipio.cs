namespace GasolinaApi.Models;

public class Municipio : EntidadBase
{
    public int DepartamentoId { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public Departamento? Departamento { get; set; }
}
