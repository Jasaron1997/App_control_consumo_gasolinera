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

    [Range(0.01, double.MaxValue, ErrorMessage = "La capacidad del tanque debe ser mayor a cero.")]
    public decimal? TanqueCapacidadGalones { get; set; }
}
