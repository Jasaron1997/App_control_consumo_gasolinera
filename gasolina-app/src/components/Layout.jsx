import { Outlet, useLocation } from 'react-router-dom'
import NavBar from './NavBar'
import { setVistaOrigen } from '../api/httpClient'

// El nombre de la vista para X-Vista-Origen se deriva de la ruta actual en un
// solo lugar, en vez de repetir el mismo useEffect de 3 líneas en cada página.
const VISTAS_POR_RUTA = {
  '/dashboard': 'Dashboard',
  '/registrar-carga': 'RegistrarCarga',
  '/historial': 'Historial',
  '/vehiculos': 'Vehiculos'
}

export default function Layout() {
  const location = useLocation()

  // Se llama directo en el render (no en un useEffect): React ejecuta los efectos
  // de los hijos (ej. el fetch de datos de cada página) antes que los del padre,
  // así que un useEffect aquí fijaría X-Vista-Origen DESPUÉS de que la página ya
  // hubiera disparado su primera petición. setVistaOrigen solo reasigna una
  // variable de módulo — no toca el DOM ni nada que dependa del ciclo de commit —
  // así que es seguro llamarlo en el cuerpo del componente.
  setVistaOrigen(VISTAS_POR_RUTA[location.pathname] ?? location.pathname)

  return (
    <div className="flex flex-col min-h-svh">
      <NavBar />
      <main className="flex-1 p-6 max-w-[1100px] w-full mx-auto">
        <Outlet />
      </main>
    </div>
  )
}
