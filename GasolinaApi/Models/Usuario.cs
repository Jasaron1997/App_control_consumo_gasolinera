namespace GasolinaApi.Models;

public class Usuario : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int TokenVersion { get; set; }
}
