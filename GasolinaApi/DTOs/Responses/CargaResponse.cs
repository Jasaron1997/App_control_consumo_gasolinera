namespace GasolinaApi.DTOs.Responses;

public class CargaResponse
{
    public int Id { get; set; }
    public int VehiculoId { get; set; }
    public string VehiculoNombre { get; set; } = string.Empty;
    public int TipoCombustibleId { get; set; }
    public string TipoCombustibleNombre { get; set; } = string.Empty;
    public int? EstacionServicioId { get; set; }
    public string? EstacionServicioNombre { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Kilometraje { get; set; }
    public decimal? KilometrosRecorridos { get; set; }
    public bool KilometrosRecorridosAutocalculado { get; set; }
    public decimal Galones { get; set; }
    public decimal CostoTotal { get; set; }
    public decimal? PrecioPorGalon { get; set; }
    public decimal? Litros { get; set; }
    public decimal? PrecioPorLitro { get; set; }
}
