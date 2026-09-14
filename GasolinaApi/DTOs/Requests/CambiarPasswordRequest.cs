using System.ComponentModel.DataAnnotations;

namespace GasolinaApi.DTOs.Requests;

public class CambiarPasswordRequest
{
    [Required]
    public string PasswordActual { get; set; } = string.Empty;

    // BCrypt solo considera los primeros 72 bytes; tope explícito en vez de
    // dejar pasar strings arbitrariamente largos hasta el hash.
    [Required, MinLength(8), MaxLength(72)]
    public string PasswordNuevo { get; set; } = string.Empty;
}
