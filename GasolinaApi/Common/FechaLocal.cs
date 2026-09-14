namespace GasolinaApi.Common;

/// <summary>
/// Carga.Fecha se guarda como fecha-calendario local sin offset (Kind Unspecified, ej.
/// "2026-08-31T00:00:00"), igual que fechaLocalHoy() en el frontend. Cualquier código que
/// necesite "hoy" para comparar contra esa columna debe usar <see cref="Hoy"/> en vez de
/// DateTime.Now/UtcNow directamente: comparar contra un instante UTC desalinea el corte en
/// las últimas horas de cada día en husos horarios negativos (ej. Guatemala, UTC-6), un bug
/// que ya se introdujo por separado en 3 lugares distintos antes de centralizarlo aquí.
/// </summary>
public static class FechaLocal
{
    // Esta app es para un solo usuario en Guatemala, que no observa horario de verano, así
    // que el offset es fijo todo el año. DateTime.Now dependía de la zona horaria
    // configurada en el sistema operativo del servidor (casi siempre UTC en contenedores/
    // hosting en la nube), no de la del usuario — exactamente la misma clase de bug que
    // esta clase existe para evitar, solo que a nivel de host en vez de a nivel de código.
    private static readonly TimeSpan OffsetGuatemala = TimeSpan.FromHours(-6);

    public static DateTime Hoy => DateTime.UtcNow.Add(OffsetGuatemala).Date;
}
