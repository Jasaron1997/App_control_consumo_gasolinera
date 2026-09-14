import { useEffect, useState } from 'react'
import { obtenerVehiculos } from '../api/vehiculosApi'

export default function SelectorVehiculo({ value, onChange }) {
  const [vehiculos, setVehiculos] = useState([])

  useEffect(() => {
    obtenerVehiculos().then(setVehiculos).catch(() => {})
  }, [])

  return (
    <select value={value} onChange={(evento) => onChange(evento.target.value)}>
      <option value="">Todos los vehículos</option>
      {vehiculos.map((vehiculo) => (
        <option key={vehiculo.id} value={vehiculo.id}>
          {vehiculo.nombre}
        </option>
      ))}
    </select>
  )
}
