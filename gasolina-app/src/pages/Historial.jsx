import { useEffect, useRef, useState } from 'react'
import { obtenerCargas, eliminarCarga } from '../api/cargasApi'
import { obtenerVehiculos } from '../api/vehiculosApi'
import { formatearFecha } from '../utils/fecha'

export default function Historial() {
  const [vehiculos, setVehiculos] = useState([])
  const [vehiculoId, setVehiculoId] = useState('')
  const [cargas, setCargas] = useState([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState(null)
  const idSolicitudRef = useRef(0)

  useEffect(() => {
    obtenerVehiculos().then(setVehiculos).catch(() => {})
  }, [])

  // idSolicitudRef descarta una respuesta que llega fuera de orden (ej. el usuario
  // cambia el filtro de vehículo dos veces rápido y la primera solicitud resuelve
  // después de la segunda), igual que el patrón ya usado en SelectorEstacion.
  async function cargarHistorial() {
    const idSolicitud = ++idSolicitudRef.current
    setCargando(true)
    try {
      const datos = await obtenerCargas(vehiculoId ? { vehiculoId } : {})
      if (idSolicitud !== idSolicitudRef.current) return
      setCargas(datos)
      setError(null)
    } catch {
      if (idSolicitud === idSolicitudRef.current) {
        setError('No se pudo cargar el historial. Intenta de nuevo.')
      }
    } finally {
      if (idSolicitud === idSolicitudRef.current) {
        setCargando(false)
      }
    }
  }

  useEffect(() => {
    cargarHistorial()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [vehiculoId])

  async function manejarEliminar(id) {
    if (!window.confirm('¿Eliminar esta carga?')) return
    setError(null)
    try {
      await eliminarCarga(id)
    } catch {
      setError('No se pudo eliminar la carga. Intenta de nuevo.')
      return
    }

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

      {error && <p className="mensaje-error">{error}</p>}

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
