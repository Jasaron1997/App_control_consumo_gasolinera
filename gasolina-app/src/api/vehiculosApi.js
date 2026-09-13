import { httpClient } from './httpClient'

export async function obtenerVehiculos() {
  const { data } = await httpClient.get('/api/vehiculos')
  return data.datos
}

export async function crearVehiculo(vehiculo) {
  const { data } = await httpClient.post('/api/vehiculos', vehiculo)
  return data.datos
}

export async function actualizarVehiculo(id, vehiculo) {
  const { data } = await httpClient.put(`/api/vehiculos/${id}`, vehiculo)
  return data.datos
}

export async function eliminarVehiculo(id) {
  await httpClient.delete(`/api/vehiculos/${id}`)
}
