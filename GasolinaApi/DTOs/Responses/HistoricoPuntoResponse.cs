namespace GasolinaApi.DTOs.Responses;

public class HistoricoPuntoResponse
{
    public DateTime Fecha { get; set; }
    public decimal? Rendimiento { get; set; }
    public decimal CostoTotal { get; set; }
}
