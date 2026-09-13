namespace GasolinaApi.DTOs.Responses;

public class MunicipioResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int DepartamentoId { get; set; }
    public string DepartamentoNombre { get; set; } = string.Empty;
}
