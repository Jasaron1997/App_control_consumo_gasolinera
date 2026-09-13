using System.ComponentModel.DataAnnotations;

namespace GasolinaApi.DTOs.Requests;

public class CrearCargaRequest
{
    [Required]
    public int VehiculoId { get; set; }

    // Opcional: si no viene, el Service la completa con el combustible por defecto del vehículo.
    public int? TipoCombustibleId { get; set; }

    public int? EstacionServicioId { get; set; }

    // Opcional: si no viene, el Service usa la fecha actual (UTC).
    public DateTime? Fecha { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "El kilometraje no puede ser negativo.")]
    public decimal Kilometraje { get; set; }

    // Opcional: si no viene, el Service la calcula restando el kilometraje de la carga anterior.
    [Range(0, double.MaxValue, ErrorMessage = "Los kilómetros recorridos no pueden ser negativos.")]
    public decimal? KilometrosRecorridos { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Los galones deben ser mayores a cero.")]
    public decimal Galones { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El costo total debe ser mayor a cero.")]
    public decimal CostoTotal { get; set; }
}
