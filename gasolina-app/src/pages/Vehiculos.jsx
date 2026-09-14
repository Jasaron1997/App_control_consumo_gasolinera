import { useEffect, useState } from 'react'
import { obtenerVehiculos, crearVehiculo, eliminarVehiculo } from '../api/vehiculosApi'
import { obtenerTiposVehiculo, obtenerTiposCombustible } from '../api/catalogosApi'
import { useConfirmar } from '../hooks/useConfirmar'

const valoresIniciales = {
  tipoVehiculoId: '',
  tipoCombustibleId: '',
  nombre: '',
  placa: '',
  tanqueCapacidadGalones: ''
}

export default function Vehiculos() {
  const [vehiculos, setVehiculos] = useState([])
  const [tiposVehiculo, setTiposVehiculo] = useState([])
  const [tiposCombustible, setTiposCombustible] = useState([])
  const [valores, setValores] = useState(valoresIniciales)
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState(null)
  const { confirmar, dialogo } = useConfirmar()

  async function cargarVehiculos() {
    try {
      const datos = await obtenerVehiculos()
      setVehiculos(datos)
    } catch {
      setError('No se pudo cargar la lista de vehículos.')
    }
  }

  useEffect(() => {
    cargarVehiculos()
    obtenerTiposVehiculo().then(setTiposVehiculo).catch(() => {})
    obtenerTiposCombustible().then(setTiposCombustible).catch(() => {})
  }, [])

  function actualizarCampo(campo, valor) {
    setValores((anteriores) => ({ ...anteriores, [campo]: valor }))
  }

  async function manejarEnvio(evento) {
    evento.preventDefault()
    setError(null)
    setGuardando(true)

    try {
      await crearVehiculo({
        tipoVehiculoId: Number(valores.tipoVehiculoId),
        tipoCombustibleId: Number(valores.tipoCombustibleId),
        nombre: valores.nombre,
        placa: valores.placa || null,
        tanqueCapacidadGalones: valores.tanqueCapacidadGalones ? Number(valores.tanqueCapacidadGalones) : null
      })
      setValores(valoresIniciales)
    } catch (error) {
      setError(error.response?.data?.mensaje ?? 'No se pudo guardar el vehículo.')
      setGuardando(false)
      return
    }

    setGuardando(false)
    // El vehículo ya se guardó; cargarVehiculos() maneja su propio error si esta
    // recarga falla (no es un error de guardado).
    cargarVehiculos()
  }

  async function manejarEliminar(id) {
    if (!(await confirmar('¿Eliminar este vehículo? El historial de cargas se conserva.'))) return
    setError(null)
    try {
      await eliminarVehiculo(id)
    } catch {
      setError('No se pudo eliminar el vehículo. Intenta de nuevo.')
      return
    }

    // El vehículo ya se eliminó; cargarVehiculos() maneja su propio error si
    // esta recarga falla (no es un error de eliminación).
    cargarVehiculos()
  }

  return (
    <div>
      {dialogo}

      <h1>Vehículos</h1>

      <ul className="list-none p-0 mb-6 flex flex-col gap-2">
        {vehiculos.map((vehiculo) => (
          <li
            key={vehiculo.id}
            className="flex justify-between items-center bg-superficie border border-borde rounded-lg px-3.5 py-2.5"
          >
            <div>
              <strong>{vehiculo.nombre}</strong>
              <span> · {vehiculo.tipoVehiculoNombre} · {vehiculo.tipoCombustibleNombre}</span>
              {vehiculo.placa && <span> · {vehiculo.placa}</span>}
            </div>
            <button type="button" className="boton-peligro" onClick={() => manejarEliminar(vehiculo.id)}>
              Eliminar
            </button>
          </li>
        ))}
      </ul>

      <h2>Agregar vehículo</h2>

      <form
        className="flex flex-col gap-3.5 max-w-[420px] bg-superficie border border-borde rounded-[10px] p-5"
        onSubmit={manejarEnvio}
      >
        <label>
          Nombre
          <input
            type="text"
            value={valores.nombre}
            onChange={(evento) => actualizarCampo('nombre', evento.target.value)}
            required
          />
        </label>

        <label>
          Tipo de vehículo
          <select
            value={valores.tipoVehiculoId}
            onChange={(evento) => actualizarCampo('tipoVehiculoId', evento.target.value)}
            required
          >
            <option value="" disabled>
              Selecciona un tipo
            </option>
            {tiposVehiculo.map((tipo) => (
              <option key={tipo.id} value={tipo.id}>
                {tipo.nombre}
              </option>
            ))}
          </select>
        </label>

        <label>
          Combustible por defecto
          <select
            value={valores.tipoCombustibleId}
            onChange={(evento) => actualizarCampo('tipoCombustibleId', evento.target.value)}
            required
          >
            <option value="" disabled>
              Selecciona un combustible
            </option>
            {tiposCombustible.map((tipo) => (
              <option key={tipo.id} value={tipo.id}>
                {tipo.nombre}
              </option>
            ))}
          </select>
        </label>

        <label>
          Placa (opcional)
          <input
            type="text"
            value={valores.placa}
            onChange={(evento) => actualizarCampo('placa', evento.target.value)}
          />
        </label>

        <label>
          Capacidad del tanque en galones (opcional)
          <input
            type="number"
            step="0.01"
            min="0"
            value={valores.tanqueCapacidadGalones}
            onChange={(evento) => actualizarCampo('tanqueCapacidadGalones', evento.target.value)}
          />
        </label>

        {error && <p className="mensaje-error">{error}</p>}

        <button type="submit" disabled={guardando}>
          {guardando ? 'Guardando...' : 'Agregar vehículo'}
        </button>
      </form>
    </div>
  )
}
