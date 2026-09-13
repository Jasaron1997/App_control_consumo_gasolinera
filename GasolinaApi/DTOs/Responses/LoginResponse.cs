namespace GasolinaApi.DTOs.Responses;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
}
