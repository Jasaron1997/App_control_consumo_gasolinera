using System.ComponentModel.DataAnnotations;

namespace GasolinaApi.DTOs.Requests;

public class EstacionServicioRequest
{
    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Marca { get; set; }

    public int? MunicipioId { get; set; }

    [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
    public decimal? Latitud { get; set; }

    [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
    public decimal? Longitud { get; set; }

    [MaxLength(255)]
    public string? Referencia { get; set; }
}
