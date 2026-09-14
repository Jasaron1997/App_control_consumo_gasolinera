import { NavLink } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

// NavLink no acepta selectores CSS como ".active" en className: la variante
// activa se decide en JS con el callback ({ isActive }) que le pasa React
// Router, no con una utilidad de Tailwind.
function claseEnlace({ isActive }) {
  return `no-underline font-medium ${isActive ? 'text-acento' : 'text-texto-sutil'}`
}

export default function NavBar() {
  const { nombreUsuario, cerrarSesion } = useAuth()

  return (
    <nav className="flex justify-between items-center flex-wrap gap-3 py-3 px-6 bg-superficie border-b border-borde">
      <div className="flex gap-4 flex-wrap">
        <NavLink to="/dashboard" className={claseEnlace}>
          Dashboard
        </NavLink>
        <NavLink to="/registrar-carga" className={claseEnlace}>
          Registrar carga
        </NavLink>
        <NavLink to="/historial" className={claseEnlace}>
          Historial
        </NavLink>
        <NavLink to="/vehiculos" className={claseEnlace}>
          Vehículos
        </NavLink>
      </div>
      <div className="flex items-center gap-3">
        <span>{nombreUsuario}</span>
        <button type="button" onClick={cerrarSesion}>
          Cerrar sesión
        </button>
      </div>
    </nav>
  )
}
