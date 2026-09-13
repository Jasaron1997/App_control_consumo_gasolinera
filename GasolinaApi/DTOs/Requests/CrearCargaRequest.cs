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
    public decimal Kilometraje { get; set; }

    // Opcional: si no viene, el Service la calcula restando el kilometraje de la carga anterior.
    public decimal? KilometrosRecorridos { get; set; }

    [Required]
    public decimal Galones { get; set; }

    [Required]
    public decimal CostoTotal { get; set; }
}
