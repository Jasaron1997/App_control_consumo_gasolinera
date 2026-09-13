import { useEffect } from 'react'
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

  useEffect(() => {
    setVistaOrigen(VISTAS_POR_RUTA[location.pathname] ?? location.pathname)
  }, [location.pathname])

  return (
    <div className="layout">
      <NavBar />
      <main className="layout__contenido">
        <Outlet />
      </main>
    </div>
  )
}
