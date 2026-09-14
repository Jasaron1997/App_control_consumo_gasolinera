import { useCallback, useEffect, useRef, useState } from 'react'
import { obtenerCargas, eliminarCarga, actualizarCarga } from '../api/cargasApi'
import { obtenerVehiculos } from '../api/vehiculosApi'
import { obtenerTiposCombustible } from '../api/catalogosApi'
import SelectorVehiculo from '../components/SelectorVehiculo'
import FormularioCarga from '../components/FormularioCarga'
import { useConfirmar } from '../hooks/useConfirmar'
import { formatearFecha } from '../utils/fecha'

// Reutilizada en cada <th>/<td> de la tabla — evita repetir la misma cadena
// larga ~20 veces (es exclusiva de este archivo, no un selector compartido
// entre archivos, así que no aplica la regla de "migrar todos juntos").
const celda = 'py-2.5 px-3 text-left border-b border-borde text-[0.9rem]'

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
  const { confirmar, dialogo } = useConfirmar()

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
    if (!(await confirmar('¿Eliminar esta carga?'))) return
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
    <div>
      {dialogo}

      <div className="flex justify-between items-center flex-wrap gap-3 mb-4">
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
        <table className="w-full border-collapse bg-superficie border border-borde rounded-[10px] overflow-hidden">
          <thead>
            <tr>
              <th className={celda}>Fecha</th>
              <th className={celda}>Vehículo</th>
              <th className={celda}>Combustible</th>
              <th className={celda}>Estación</th>
              <th className={celda}>Kilometraje</th>
              <th className={celda}>Km recorridos</th>
              <th className={celda}>Galones</th>
              <th className={celda}>Costo total</th>
              <th className={celda}>Q/galón</th>
              <th className={celda}></th>
            </tr>
          </thead>
          <tbody>
            {cargas.map((carga) => (
              <tr key={carga.id}>
                <td className={celda}>{formatearFecha(carga.fecha)}</td>
                <td className={celda}>{carga.vehiculoNombre}</td>
                <td className={celda}>{carga.tipoCombustibleNombre}</td>
                <td className={celda}>{carga.estacionServicioNombre ?? '—'}</td>
                <td className={celda}>{carga.kilometraje}</td>
                <td className={celda}>{carga.kilometrosRecorridos ?? '—'}</td>
                <td className={celda}>{carga.galones}</td>
                <td className={celda}>Q{carga.costoTotal.toFixed(2)}</td>
                <td className={celda}>{carga.precioPorGalon?.toFixed(2) ?? '—'}</td>
                <td className={celda}>
                  <div className="flex gap-2">
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
