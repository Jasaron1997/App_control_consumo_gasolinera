import { useCallback, useEffect, useRef, useState } from 'react'
import { obtenerCargas, eliminarCarga, actualizarCarga } from '../api/cargasApi'
import { obtenerVehiculos } from '../api/vehiculosApi'
import { obtenerTiposCombustible } from '../api/catalogosApi'
import SelectorVehiculo from '../components/SelectorVehiculo'
import FormularioCarga from '../components/FormularioCarga'
import { formatearFecha } from '../utils/fecha'

export default function Historial() {
  const [vehiculoId, setVehiculoId] = useState('')
  const [cargas, setCargas] = useState([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState(null)
  const idSolicitudRef = useRef(0)

  // Solo se usan cuando se está editando una carga (formulario de edición).
  const [vehiculos, setVehiculos] = useState([])
  const [tiposCombustible, setTiposCombustible] = useState([])
  const [cargaEditando, setCargaEditando] = useState(null)
  const [guardandoEdicion, setGuardandoEdicion] = useState(false)

  // idSolicitudRef descarta una respuesta que llega fuera de orden (ej. el usuario
  // cambia el filtro de vehículo dos veces rápido y la primera solicitud resuelve
  // después de la segunda), igual que el patrón ya usado en SelectorEstacion.
  const cargarHistorial = useCallback(async () => {
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
  }, [vehiculoId])

  useEffect(() => {
    cargarHistorial()
  }, [cargarHistorial])

  // Los catálogos para el formulario de edición se cargan una sola vez, al
  // montar, igual que en RegistrarCarga.jsx — no dependen del filtro de vehículo.
  useEffect(() => {
    obtenerVehiculos().then(setVehiculos).catch(() => {})
    obtenerTiposCombustible().then(setTiposCombustible).catch(() => {})
  }, [])

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

  async function manejarGuardarEdicion(payload) {
    setGuardandoEdicion(true)
    try {
      await actualizarCarga(cargaEditando.id, payload)
      setCargaEditando(null)
      cargarHistorial()
    } finally {
      setGuardandoEdicion(false)
    }
  }

  return (
    <div className="pagina-historial">
      <div className="pagina-historial__encabezado">
        <h1>Historial de cargas</h1>

        <SelectorVehiculo value={vehiculoId} onChange={setVehiculoId} />
      </div>

      {error && <p className="mensaje-error">{error}</p>}

      {cargaEditando && (
        <FormularioCarga
          vehiculos={vehiculos}
          tiposCombustible={tiposCombustible}
          cargaExistente={cargaEditando}
          onGuardar={manejarGuardarEdicion}
          onCancelar={() => setCargaEditando(null)}
          guardando={guardandoEdicion}
        />
      )}

      {!cargaEditando && (cargando ? (
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
                  <div className="tabla-historial__acciones">
                    <button type="button" onClick={() => setCargaEditando(carga)}>
                      Editar
                    </button>
                    <button
                      type="button"
                      className="boton-peligro"
                      onClick={() => manejarEliminar(carga.id)}
                    >
                      Eliminar
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      ))}
    </div>
  )
}
