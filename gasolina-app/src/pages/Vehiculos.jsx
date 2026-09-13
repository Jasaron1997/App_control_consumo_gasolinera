import { useEffect, useState } from 'react'
import { obtenerVehiculos, crearVehiculo, eliminarVehiculo } from '../api/vehiculosApi'
import { obtenerTiposVehiculo, obtenerTiposCombustible } from '../api/catalogosApi'
import { setVistaOrigen } from '../api/httpClient'

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

  useEffect(() => {
    setVistaOrigen('Vehiculos')
  }, [])

  async function cargarVehiculos() {
    const datos = await obtenerVehiculos()
    setVehiculos(datos)
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
      await cargarVehiculos()
    } catch (error) {
      setError(error.response?.data?.mensaje ?? 'No se pudo guardar el vehículo.')
    } finally {
      setGuardando(false)
    }
  }

  async function manejarEliminar(id) {
    if (!window.confirm('¿Eliminar este vehículo? El historial de cargas se conserva.')) return
    await eliminarVehiculo(id)
    await cargarVehiculos()
  }

  return (
    <div className="pagina-vehiculos">
      <h1>Vehículos</h1>

      <ul className="lista-vehiculos">
        {vehiculos.map((vehiculo) => (
          <li key={vehiculo.id}>
            <div>
              <strong>{vehiculo.nombre}</strong>
              <span> · {vehiculo.tipoVehiculoNombre} · {vehiculo.tipoCombustibleNombre}</span>
              {vehiculo.placa && <span> · {vehiculo.placa}</span>}
            </div>
            <button type="button" onClick={() => manejarEliminar(vehiculo.id)}>
              Eliminar
            </button>
          </li>
        ))}
      </ul>

      <h2>Agregar vehículo</h2>

      <form className="formulario-vehiculo" onSubmit={manejarEnvio}>
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
