namespace GasolinaApi.Models;

public class Carga : EntidadBase
{
    public int VehiculoId { get; set; }
    public int TipoCombustibleId { get; set; }
    public int? EstacionServicioId { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Kilometraje { get; set; }
    public decimal? KilometrosRecorridos { get; set; }
    // true si KilometrosRecorridos lo calculó CargaService (kilometraje - el de la
    // carga anterior); false si el usuario lo escribió a mano. Distingue cuándo es
    // seguro recalcularlo solo al editar/borrar una carga vecina, y cuándo no
    // tocarlo porque el usuario lo fijó a propósito.
    public bool KilometrosRecorridosAutocalculado { get; set; }
    public decimal Galones { get; set; }
    public decimal CostoTotal { get; set; }

    // Columnas calculadas PERSISTED en BD: el código nunca las escribe, solo las lee.
    public decimal? PrecioPorGalon { get; private set; }
    public decimal? Litros { get; private set; }
    public decimal? PrecioPorLitro { get; private set; }

    public Vehiculo? Vehiculo { get; set; }
    public TipoCombustible? TipoCombustible { get; set; }
    public EstacionServicio? EstacionServicio { get; set; }
}
