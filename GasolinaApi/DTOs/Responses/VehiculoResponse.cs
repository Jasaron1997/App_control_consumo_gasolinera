namespace GasolinaApi.DTOs.Responses;

public class VehiculoResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Placa { get; set; }
    public decimal? TanqueCapacidadGalones { get; set; }
    public int TipoVehiculoId { get; set; }
    public string TipoVehiculoNombre { get; set; } = string.Empty;
    public int TipoCombustibleId { get; set; }
    public string TipoCombustibleNombre { get; set; } = string.Empty;
}
