import { useEffect, useState } from 'react'
import { obtenerCargas, eliminarCarga } from '../api/cargasApi'
import { obtenerVehiculos } from '../api/vehiculosApi'
import { setVistaOrigen } from '../api/httpClient'
import { formatearFecha } from '../utils/fecha'

export default function Historial() {
  const [vehiculos, setVehiculos] = useState([])
  const [vehiculoId, setVehiculoId] = useState('')
  const [cargas, setCargas] = useState([])
  const [cargando, setCargando] = useState(true)

  useEffect(() => {
    setVistaOrigen('Historial')
  }, [])

  useEffect(() => {
    obtenerVehiculos().then(setVehiculos).catch(() => {})
  }, [])

  async function cargarHistorial() {
    setCargando(true)
    try {
      const datos = await obtenerCargas(vehiculoId ? { vehiculoId } : {})
      setCargas(datos)
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => {
    cargarHistorial()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [vehiculoId])

  async function manejarEliminar(id) {
    if (!window.confirm('¿Eliminar esta carga?')) return
    await eliminarCarga(id)
    cargarHistorial()
  }

  return (
    <div className="pagina-historial">
      <div className="pagina-historial__encabezado">
        <h1>Historial de cargas</h1>

        <select value={vehiculoId} onChange={(evento) => setVehiculoId(evento.target.value)}>
          <option value="">Todos los vehículos</option>
          {vehiculos.map((vehiculo) => (
            <option key={vehiculo.id} value={vehiculo.id}>
              {vehiculo.nombre}
            </option>
          ))}
        </select>
      </div>

      {cargando ? (
        <p>Cargando...</p>
      ) : cargas.length === 0 ? (
        <p>No hay cargas registradas todavía.</p>
      ) : (
        <table className="tabla-historial">
          <thead>
            <tr>
              <th>Fecha</th>
              <th>Vehículo</th>
              <th>Combustible</th>
              <th>Estación</th>
              <th>Kilometraje</th>
              <th>Km recorridos</th>
              <th>Galones</th>
              <th>Costo total</th>
              <th>Q/galón</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {cargas.map((carga) => (
              <tr key={carga.id}>
                <td>{formatearFecha(carga.fecha)}</td>
                <td>{carga.vehiculoNombre}</td>
                <td>{carga.tipoCombustibleNombre}</td>
                <td>{carga.estacionServicioNombre ?? '—'}</td>
                <td>{carga.kilometraje}</td>
                <td>{carga.kilometrosRecorridos ?? '—'}</td>
                <td>{carga.galones}</td>
                <td>Q{carga.costoTotal.toFixed(2)}</td>
                <td>{carga.precioPorGalon?.toFixed(2) ?? '—'}</td>
                <td>
                  <button type="button" onClick={() => manejarEliminar(carga.id)}>
                    Eliminar
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
