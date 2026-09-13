namespace GasolinaApi.Models;

public class Vehiculo : EntidadBase
{
    public int TipoVehiculoId { get; set; }
    public int TipoCombustibleId { get; set; }
    public int UsuarioPropietarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Placa { get; set; }
    public decimal? TanqueCapacidadGalones { get; set; }

    public TipoVehiculo? TipoVehiculo { get; set; }
    public TipoCombustible? TipoCombustible { get; set; }
    public Usuario? UsuarioPropietario { get; set; }
}
