import { useState } from 'react'
import SelectorEstacion from './SelectorEstacion'

function valoresIniciales() {
  return {
    vehiculoId: '',
    tipoCombustibleId: '',
    fecha: new Date().toISOString().slice(0, 10),
    kilometraje: '',
    kilometrosRecorridos: '',
    galones: '',
    costoTotal: ''
  }
}

export default function FormularioCarga({ vehiculos, tiposCombustible, onGuardar, guardando }) {
  const [valores, setValores] = useState(valoresIniciales)
  const [estacionSeleccionada, setEstacionSeleccionada] = useState(null)
  const [error, setError] = useState(null)

  function actualizarCampo(campo, valor) {
    setValores((anteriores) => ({ ...anteriores, [campo]: valor }))
  }

  function manejarCambioVehiculo(vehiculoId) {
    const vehiculo = vehiculos.find((v) => String(v.id) === vehiculoId)

    setValores((anteriores) => ({
      ...anteriores,
      vehiculoId,
      tipoCombustibleId: vehiculo ? String(vehiculo.tipoCombustibleId) : anteriores.tipoCombustibleId
    }))
  }

  async function manejarEnvio(evento) {
    evento.preventDefault()
    setError(null)

    try {
      await onGuardar({
        vehiculoId: Number(valores.vehiculoId),
        tipoCombustibleId: valores.tipoCombustibleId ? Number(valores.tipoCombustibleId) : null,
        estacionServicioId: estacionSeleccionada?.id ?? null,
        fecha: valores.fecha ? new Date(valores.fecha).toISOString() : null,
        kilometraje: Number(valores.kilometraje),
        kilometrosRecorridos: valores.kilometrosRecorridos ? Number(valores.kilometrosRecorridos) : null,
        galones: Number(valores.galones),
        costoTotal: Number(valores.costoTotal)
      })

      setValores(valoresIniciales())
      setEstacionSeleccionada(null)
    } catch (error) {
      setError(error.response?.data?.mensaje ?? 'No se pudo registrar la carga.')
    }
  }

  return (
    <form className="formulario-carga" onSubmit={manejarEnvio}>
      <label>
        Vehículo
        <select
          value={valores.vehiculoId}
          onChange={(evento) => manejarCambioVehiculo(evento.target.value)}
          required
        >
          <option value="" disabled>
            Selecciona un vehículo
          </option>
          {vehiculos.map((vehiculo) => (
            <option key={vehiculo.id} value={vehiculo.id}>
              {vehiculo.nombre}
            </option>
          ))}
        </select>
      </label>

      <label>
        Combustible
        <select
          value={valores.tipoCombustibleId}
          onChange={(evento) => actualizarCampo('tipoCombustibleId', evento.target.value)}
        >
          <option value="">Usar el del vehículo</option>
          {tiposCombustible.map((tipo) => (
            <option key={tipo.id} value={tipo.id}>
              {tipo.nombre}
            </option>
          ))}
        </select>
      </label>

      <SelectorEstacion estacionSeleccionada={estacionSeleccionada} onSeleccionar={setEstacionSeleccionada} />

      <label>
        Fecha
        <input
          type="date"
          value={valores.fecha}
          onChange={(evento) => actualizarCampo('fecha', evento.target.value)}
        />
      </label>

      <label>
        Kilometraje actual
        <input
          type="number"
          step="0.01"
          min="0"
          value={valores.kilometraje}
          onChange={(evento) => actualizarCampo('kilometraje', evento.target.value)}
          required
        />
      </label>

      <label>
        Kilómetros recorridos (opcional)
        <input
          type="number"
          step="0.01"
          min="0"
          placeholder="Se calcula automáticamente si lo dejas vacío"
          value={valores.kilometrosRecorridos}
          onChange={(evento) => actualizarCampo('kilometrosRecorridos', evento.target.value)}
        />
      </label>

      <label>
        Galones
        <input
          type="number"
          step="0.01"
          min="0"
          value={valores.galones}
          onChange={(evento) => actualizarCampo('galones', evento.target.value)}
          required
        />
      </label>

      <label>
        Costo total (Q)
        <input
          type="number"
          step="0.01"
          min="0"
          value={valores.costoTotal}
          onChange={(evento) => actualizarCampo('costoTotal', evento.target.value)}
          required
        />
      </label>

      {error && <p className="mensaje-error">{error}</p>}

      <button type="submit" disabled={guardando}>
        {guardando ? 'Guardando...' : 'Registrar carga'}
      </button>
    </form>
  )
}
