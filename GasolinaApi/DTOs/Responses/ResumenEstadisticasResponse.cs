namespace GasolinaApi.DTOs.Responses;

public class ResumenEstadisticasResponse
{
    public decimal? RendimientoPromedio { get; set; }
    public decimal GastoMes { get; set; }
    public decimal? CostoPorKm { get; set; }
    public decimal KmRecorridosMes { get; set; }
}
