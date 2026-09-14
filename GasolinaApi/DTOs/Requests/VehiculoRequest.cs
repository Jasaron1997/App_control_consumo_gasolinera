using System.ComponentModel.DataAnnotations;

namespace GasolinaApi.DTOs.Requests;

public class VehiculoRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un tipo de vehículo.")]
    public int TipoVehiculoId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un tipo de combustible.")]
    public int TipoCombustibleId { get; set; }

    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Placa { get; set; }

    // Tope superior = precisión de TB_VEHICULO.TANQUE_CAPACIDAD_GALONES (DECIMAL(5,2)).
    [Range(0.01, 999.99, ErrorMessage = "La capacidad del tanque debe estar entre 0.01 y 999.99.")]
    public decimal? TanqueCapacidadGalones { get; set; }
}
