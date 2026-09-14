import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis
} from 'recharts'
import { formatearFechaCorta } from '../utils/fecha'

export default function GraficaTendencia({ datos }) {
  const datosFormateados = datos.map((punto) => ({
    ...punto,
    fechaEtiqueta: formatearFechaCorta(punto.fecha)
  }))

  return (
    <div className="grafica-tendencia">
      <ResponsiveContainer width="100%" height={320}>
        <LineChart data={datosFormateados} margin={{ top: 10, right: 20, left: 0, bottom: 0 }}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="fechaEtiqueta" />
          <YAxis yAxisId="rendimiento" />
          <YAxis yAxisId="costo" orientation="right" />
          <Tooltip />
          <Legend />
          <Line
            yAxisId="rendimiento"
            type="monotone"
            dataKey="rendimiento"
            name="Rendimiento (km/gal)"
            stroke="#2563eb"
            connectNulls
          />
          <Line
            yAxisId="costo"
            type="monotone"
            dataKey="costoTotal"
            name="Costo total (Q)"
            stroke="#16a34a"
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  )
}
