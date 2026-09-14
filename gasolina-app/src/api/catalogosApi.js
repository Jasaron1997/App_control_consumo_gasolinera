import { httpClient } from './httpClient'

export async function obtenerTiposVehiculo() {
  const { data } = await httpClient.get('/api/catalogos/tipos-vehiculo')
  return data.datos
}

export async function obtenerTiposCombustible() {
  const { data } = await httpClient.get('/api/catalogos/tipos-combustible')
  return data.datos
}

export async function buscarEstacionesServicio(buscar) {
  const { data } = await httpClient.get('/api/catalogos/estaciones-servicio', {
    params: { buscar }
  })
  return data.datos
}

export async function crearEstacionServicio(estacion) {
  const { data } = await httpClient.post('/api/catalogos/estaciones-servicio', estacion)
  return data.datos
}
