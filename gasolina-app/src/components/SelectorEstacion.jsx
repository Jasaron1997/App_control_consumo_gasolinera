import { useEffect, useRef, useState } from 'react'
import { buscarEstacionesServicio, crearEstacionServicio } from '../api/catalogosApi'

export default function SelectorEstacion({ estacionSeleccionada, onSeleccionar }) {
  const [texto, setTexto] = useState(estacionSeleccionada?.nombre ?? '')
  const [opciones, setOpciones] = useState([])
  const [mostrarOpciones, setMostrarOpciones] = useState(false)
  const [creando, setCreando] = useState(false)
  const temporizadorRef = useRef(null)
  const idSolicitudRef = useRef(0)

  useEffect(() => {
    setTexto(estacionSeleccionada?.nombre ?? '')
  }, [estacionSeleccionada])

  useEffect(() => () => clearTimeout(temporizadorRef.current), [])

  function manejarCambioTexto(valor) {
    setTexto(valor)
    onSeleccionar(null)

    clearTimeout(temporizadorRef.current)

    if (valor.trim().length < 2) {
      setOpciones([])
      return
    }

    temporizadorRef.current = setTimeout(async () => {
      const idSolicitud = ++idSolicitudRef.current

      try {
        const resultados = await buscarEstacionesServicio(valor)
        // Descarta la respuesta si ya salió una búsqueda más nueva mientras esta estaba en vuelo.
        if (idSolicitud !== idSolicitudRef.current) return
        setOpciones(resultados)
        setMostrarOpciones(true)
      } catch {
        if (idSolicitud === idSolicitudRef.current) setOpciones([])
      }
    }, 300)
  }

  function seleccionarOpcion(estacion) {
    setTexto(estacion.nombre)
    setMostrarOpciones(false)
    onSeleccionar(estacion)
  }

  async function crearNuevaEstacion() {
    setCreando(true)
    try {
      const nuevaEstacion = await crearEstacionServicio({ nombre: texto })
      seleccionarOpcion(nuevaEstacion)
    } finally {
      setCreando(false)
    }
  }

  return (
    <div className="selector-estacion">
      <label>
        Estación de servicio
        <input
          type="text"
          value={texto}
          placeholder="Buscar o escribir una nueva..."
          onChange={(evento) => manejarCambioTexto(evento.target.value)}
          onFocus={() => setMostrarOpciones(opciones.length > 0)}
          onBlur={() => setTimeout(() => setMostrarOpciones(false), 150)}
        />
      </label>

      {mostrarOpciones && (
        <ul className="selector-estacion__opciones">
          {opciones.map((estacion) => (
            <li key={estacion.id}>
              <button type="button" onClick={() => seleccionarOpcion(estacion)}>
                {estacion.nombre}
                {estacion.marca && <span> · {estacion.marca}</span>}
              </button>
            </li>
          ))}

          {opciones.length === 0 && texto.trim().length >= 2 && (
            <li>
              <button type="button" disabled={creando} onClick={crearNuevaEstacion}>
                {creando ? 'Creando...' : `Crear "${texto}"`}
              </button>
            </li>
          )}
        </ul>
      )}
    </div>
  )
}
