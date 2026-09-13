using System.ComponentModel.DataAnnotations;

namespace GasolinaApi.DTOs.Requests;

public class CambiarPasswordRequest
{
    [Required]
    public string PasswordActual { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string PasswordNuevo { get; set; } = string.Empty;
}
