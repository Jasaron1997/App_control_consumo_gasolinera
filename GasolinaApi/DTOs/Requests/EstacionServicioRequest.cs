using System.ComponentModel.DataAnnotations;

namespace GasolinaApi.DTOs.Requests;

public class EstacionServicioRequest
{
    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Marca { get; set; }

    public int? MunicipioId { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    [MaxLength(255)]
    public string? Referencia { get; set; }
}
