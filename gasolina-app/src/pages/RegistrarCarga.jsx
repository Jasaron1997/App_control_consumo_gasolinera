import { useEffect, useState } from 'react'
import FormularioCarga from '../components/FormularioCarga'
import { crearCarga } from '../api/cargasApi'
import { obtenerVehiculos } from '../api/vehiculosApi'
import { obtenerTiposCombustible } from '../api/catalogosApi'

export default function RegistrarCarga() {
  const [vehiculos, setVehiculos] = useState([])
  const [tiposCombustible, setTiposCombustible] = useState([])
  const [guardando, setGuardando] = useState(false)
  const [mensajeExito, setMensajeExito] = useState(null)

  useEffect(() => {
    obtenerVehiculos().then(setVehiculos).catch(() => {})
    obtenerTiposCombustible().then(setTiposCombustible).catch(() => {})
  }, [])

  async function manejarGuardar(carga) {
    setGuardando(true)
    setMensajeExito(null)

    try {
      await crearCarga(carga)
      setMensajeExito('Carga registrada correctamente.')
    } finally {
      setGuardando(false)
    }
  }

  return (
    <div>
      <h1>Registrar carga</h1>

      {mensajeExito && <p className="mensaje-exito">{mensajeExito}</p>}

      {vehiculos.length === 0 ? (
        <p>Primero registra un vehículo para poder cargar combustible.</p>
      ) : (
        <FormularioCarga
          vehiculos={vehiculos}
          tiposCombustible={tiposCombustible}
          onGuardar={manejarGuardar}
          guardando={guardando}
        />
      )}
    </div>
  )
}
