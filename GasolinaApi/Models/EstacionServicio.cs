namespace GasolinaApi.Models;

public class EstacionServicio : EntidadBase
{
    public int? MunicipioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public string? Referencia { get; set; }

    public Municipio? Municipio { get; set; }
}
