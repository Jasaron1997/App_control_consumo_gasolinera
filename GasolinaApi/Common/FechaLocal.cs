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
    public static DateTime Hoy => DateTime.Now.Date;
}
