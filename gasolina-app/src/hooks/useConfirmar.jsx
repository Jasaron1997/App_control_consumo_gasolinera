import { useCallback, useRef, useState } from 'react'

// Reemplaza window.confirm(): no es solo estético — un confirm() nativo
// bloquea el hilo principal, y cualquier herramienta de automatización
// (o cualquier código que dependa de que la página siga respondiendo)
// se cuelga esperando a que alguien lo cierre a mano. Este hook devuelve
// una promesa en vez de bloquear, usando <dialog> nativo (focus-trap y
// cierre con Escape gratis, sin ninguna librería nueva).
export function useConfirmar() {
  const [mensaje, setMensaje] = useState('')
  const resolverRef = useRef(null)
  const dialogRef = useRef(null)

  const confirmar = useCallback((texto) => {
    setMensaje(texto)
    dialogRef.current?.showModal()
    return new Promise((resolve) => {
      resolverRef.current = resolve
    })
  }, [])

  function responder(valor) {
    dialogRef.current?.close()
    resolverRef.current?.(valor)
  }

  const dialogo = (
    <dialog
      ref={dialogRef}
      onCancel={() => responder(false)}
      className="fixed inset-0 m-auto backdrop:bg-black/50 rounded-lg border border-borde bg-superficie text-texto p-6 shadow-lg"
    >
      <p className="mb-4">{mensaje}</p>
      <div className="flex justify-end gap-2">
        <button type="button" className="boton-secundario" onClick={() => responder(false)}>
          Cancelar
        </button>
        <button type="button" className="boton-peligro" autoFocus onClick={() => responder(true)}>
          Eliminar
        </button>
      </div>
    </dialog>
  )

  return { confirmar, dialogo }
}
