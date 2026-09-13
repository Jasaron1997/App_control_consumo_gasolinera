import { httpClient } from './httpClient'

export async function obtenerCargas({ vehiculoId, desde, hasta } = {}) {
  const { data } = await httpClient.get('/api/cargas', {
    params: { vehiculoId, desde, hasta }
  })
  return data.datos
}

export async function crearCarga(carga) {
  const { data } = await httpClient.post('/api/cargas', carga)
  return data.datos
}

export async function actualizarCarga(id, carga) {
  const { data } = await httpClient.put(`/api/cargas/${id}`, carga)
  return data.datos
}

export async function eliminarCarga(id) {
  await httpClient.delete(`/api/cargas/${id}`)
}
