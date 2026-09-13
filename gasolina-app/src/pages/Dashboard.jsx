import { useEffect, useState } from 'react'
import StatCard from '../components/StatCard'
import GraficaTendencia from '../components/GraficaTendencia'
import { obtenerResumen, obtenerHistorico } from '../api/estadisticasApi'
import { obtenerVehiculos } from '../api/vehiculosApi'
import { setVistaOrigen } from '../api/httpClient'

export default function Dashboard() {
  const [vehiculos, setVehiculos] = useState([])
  const [vehiculoId, setVehiculoId] = useState('')
  const [resumen, setResumen] = useState(null)
  const [historico, setHistorico] = useState([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    setVistaOrigen('Dashboard')
  }, [])

  useEffect(() => {
    obtenerVehiculos().then(setVehiculos).catch(() => {})
  }, [])

  useEffect(() => {
    let activo = true
    setCargando(true)
    setError(null)

    const filtro = vehiculoId ? { vehiculoId } : {}

    Promise.all([obtenerResumen(filtro), obtenerHistorico(filtro)])
      .then(([resumenData, historicoData]) => {
        if (!activo) return
        setResumen(resumenData)
        setHistorico(historicoData)
      })
      .catch(() => {
        if (activo) setError('No se pudieron cargar las estadísticas.')
      })
      .finally(() => {
        if (activo) setCargando(false)
      })

    return () => {
      activo = false
    }
  }, [vehiculoId])

  return (
    <div className="pagina-dashboard">
      <div className="pagina-dashboard__encabezado">
        <h1>Dashboard</h1>

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

      {!cargando && resumen && (
        <div className="tarjetas-resumen">
          <StatCard
            titulo="Rendimiento promedio"
            valor={resumen.rendimientoPromedio?.toFixed(2) ?? '—'}
            unidad="km/gal"
          />
          <StatCard titulo="Gasto del mes" valor={`Q${resumen.gastoMes.toFixed(2)}`} />
          <StatCard titulo="Km recorridos (mes)" valor={resumen.kmRecorridosMes.toFixed(1)} unidad="km" />
          <StatCard
            titulo="Costo por km"
            valor={resumen.costoPorKm?.toFixed(2) ?? '—'}
            unidad="Q/km"
          />
        </div>
      )}

      {!cargando && historico.length > 0 && <GraficaTendencia datos={historico} />}
      {!cargando && historico.length === 0 && <p>Todavía no hay cargas registradas para graficar.</p>}
    </div>
  )
}
