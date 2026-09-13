namespace GasolinaApi.Filters;

/// <summary>
/// Excluye puntualmente un endpoint del log de auditoría automático (ej. catálogos de consulta constante).
/// La exclusión es la excepción, no la regla.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SinAuditoriaAttribute : Attribute
{
}
