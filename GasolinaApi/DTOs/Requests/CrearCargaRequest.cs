using System.ComponentModel.DataAnnotations;

namespace GasolinaApi.DTOs.Requests;

public class CrearCargaRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un vehículo.")]
    public int VehiculoId { get; set; }

    // Opcional: si no viene, el Service la completa con el combustible por defecto del vehículo.
    public int? TipoCombustibleId { get; set; }

    public int? EstacionServicioId { get; set; }

    // Opcional: si no viene, el Service usa la fecha-calendario local de hoy (sin hora),
    // no el instante UTC — ver CargaService.CrearAsync.
    public DateTime? Fecha { get; set; }

    // Tope superior = precisión real de TB_CARGA.KILOMETRAJE (DECIMAL(10,2)); sin él,
    // un valor fuera de rango pasaba la validación y reventaba en un 500 genérico al
    // guardar en vez de un 400 claro.
    [Required]
    [Range(0, 99999999.99, ErrorMessage = "El kilometraje no puede ser negativo ni mayor a 99999999.99.")]
    public decimal Kilometraje { get; set; }

    // Opcional: si no viene, el Service la calcula restando el kilometraje de la carga anterior.
    // Tope superior = precisión de TB_CARGA.KILOMETROS_RECORRIDOS (DECIMAL(10,2)).
    [Range(0, 99999999.99, ErrorMessage = "Los kilómetros recorridos no pueden ser negativos ni mayores a 99999999.99.")]
    public decimal? KilometrosRecorridos { get; set; }

    // Tope superior = precisión de TB_CARGA.GALONES (DECIMAL(6,2)).
    [Required]
    [Range(0.01, 9999.99, ErrorMessage = "Los galones deben estar entre 0.01 y 9999.99.")]
    public decimal Galones { get; set; }

    // Tope superior = precisión de TB_CARGA.COSTO_TOTAL (DECIMAL(8,2)).
    [Required]
    [Range(0.01, 999999.99, ErrorMessage = "El costo total debe estar entre 0.01 y 999999.99.")]
    public decimal CostoTotal { get; set; }
}
