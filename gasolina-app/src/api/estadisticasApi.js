import { httpClient } from './httpClient'

export async function obtenerResumen({ vehiculoId } = {}) {
  const { data } = await httpClient.get('/api/estadisticas/resumen', {
    params: { vehiculoId }
  })
  return data.datos
}

export async function obtenerHistorico({ vehiculoId, meses = 6 } = {}) {
  const { data } = await httpClient.get('/api/estadisticas/historico', {
    params: { vehiculoId, meses }
  })
  return data.datos
}
