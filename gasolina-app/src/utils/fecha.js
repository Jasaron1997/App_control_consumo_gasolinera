// El backend serializa DateTime con Kind=Unspecified (EF Core + SQL Server no
// conservan Kind=Utc al leer un datetime2), así que el JSON llega SIN sufijo
// "Z" (ej. "2026-09-10T00:00:00"). Si se parseara con `new Date(...)`, el
// navegador lo interpretaría como hora LOCAL (no UTC), corriendo la fecha
// mostrada en cualquier dirección según el huso horario del usuario. Por eso
// estas funciones extraen año/mes/día directo del string ISO, sin pasar por
// el constructor Date.

const MESES_CORTOS = ['ene', 'feb', 'mar', 'abr', 'may', 'jun', 'jul', 'ago', 'sep', 'oct', 'nov', 'dic']

export function formatearFecha(fechaIso) {
  const [anio, mes, dia] = fechaIso.slice(0, 10).split('-')
  return `${dia}/${mes}/${anio}`
}

export function formatearFechaCorta(fechaIso) {
  const [, mes, dia] = fechaIso.slice(0, 10).split('-')
  return `${dia} ${MESES_CORTOS[Number(mes) - 1]}`
}
