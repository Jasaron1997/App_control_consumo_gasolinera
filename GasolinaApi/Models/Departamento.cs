namespace GasolinaApi.Models;

public class Departamento : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public ICollection<Municipio> Municipios { get; set; } = new List<Municipio>();
}
