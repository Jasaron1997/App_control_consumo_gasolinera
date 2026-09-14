using System.ComponentModel.DataAnnotations;

namespace GasolinaApi.DTOs.Requests;

public class LoginRequest
{
    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    // BCrypt solo considera los primeros 72 bytes; tope explícito en vez de
    // dejar pasar strings arbitrariamente largos hasta el hash.
    [Required, MaxLength(72)]
    public string Password { get; set; } = string.Empty;
}
