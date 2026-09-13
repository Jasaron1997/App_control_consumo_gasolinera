namespace GasolinaApi.DTOs.Responses;

public class EstacionServicioResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public int? MunicipioId { get; set; }
    public string? MunicipioNombre { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public string? Referencia { get; set; }
}
