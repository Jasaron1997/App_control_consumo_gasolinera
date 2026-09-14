import { useState } from 'react'
import SelectorEstacion from './SelectorEstacion'

// new Date().toISOString() da la fecha en UTC: entre las 18:00 y medianoche hora
// de Guatemala (UTC-6) ya sería "mañana" en UTC. Se arma el string con los
// componentes de fecha LOCALES para que el valor por defecto sea el día real.
function fechaLocalHoy() {
  const ahora = new Date()
  const anio = ahora.getFullYear()
  const mes = String(ahora.getMonth() + 1).padStart(2, '0')
  const dia = String(ahora.getDate()).padStart(2, '0')
  return `${anio}-${mes}-${dia}`
}

// Sin cargaExistente: valores en blanco para registrar una carga nueva. Con
// cargaExistente: precarga el formulario para editarla. "Kilómetros
// recorridos" se deja en blanco cuando ese valor lo calculó la app sola
// (kilometrosRecorridosAutocalculado) para que, si no se toca, siga
// autocalculándose con los datos nuevos en vez de "congelarlo" con el valor
// viejo — solo se precarga cuando el usuario lo había puesto a mano.
function valoresIniciales(cargaExistente) {
  if (!cargaExistente) {
    return {
      vehiculoId: '',
      tipoCombustibleId: '',
      fecha: fechaLocalHoy(),
      kilometraje: '',
      kilometrosRecorridos: '',
      galones: '',
      costoTotal: ''
    }
  }

  return {
    vehiculoId: String(cargaExistente.vehiculoId),
    tipoCombustibleId: String(cargaExistente.tipoCombustibleId),
    fecha: cargaExistente.fecha.slice(0, 10),
    kilometraje: String(cargaExistente.kilometraje),
    kilometrosRecorridos: cargaExistente.kilometrosRecorridosAutocalculado
      ? ''
      : String(cargaExistente.kilometrosRecorridos ?? ''),
    galones: String(cargaExistente.galones),
    costoTotal: String(cargaExistente.costoTotal)
  }
}

function estacionInicial(cargaExistente) {
  if (!cargaExistente?.estacionServicioId) return null
  return { id: cargaExistente.estacionServicioId, nombre: cargaExistente.estacionServicioNombre }
}

export default function FormularioCarga({ vehiculos, tiposCombustible, onGuardar, guardando, cargaExistente, onCancelar }) {
  const [valores, setValores] = useState(() => valoresIniciales(cargaExistente))
  const [estacionSeleccionada, setEstacionSeleccionada] = useState(() => estacionInicial(cargaExistente))
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

      // En modo edición no se limpia el formulario: el padre cierra la vista de
      // edición al terminar (ver Historial.jsx). Reiniciarlo aquí igual pisaría
      // los valores justo antes de que el formulario se desmonte.
      if (!cargaExistente) {
        setValores(valoresIniciales())
        setEstacionSeleccionada(null)
      }
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

      <div className="formulario-carga__acciones">
        <button type="submit" disabled={guardando}>
          {guardando
            ? 'Guardando...'
            : cargaExistente
              ? 'Guardar cambios'
              : 'Registrar carga'}
        </button>

        {onCancelar && (
          <button type="button" className="boton-secundario" onClick={onCancelar} disabled={guardando}>
            Cancelar
          </button>
        )}
      </div>
    </form>
  )
}
