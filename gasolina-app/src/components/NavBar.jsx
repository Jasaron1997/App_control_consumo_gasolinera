import { NavLink } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function NavBar() {
  const { nombreUsuario, cerrarSesion } = useAuth()

  return (
    <nav className="nav-bar">
      <div className="nav-bar__enlaces">
        <NavLink to="/dashboard">Dashboard</NavLink>
        <NavLink to="/registrar-carga">Registrar carga</NavLink>
        <NavLink to="/historial">Historial</NavLink>
        <NavLink to="/vehiculos">Vehículos</NavLink>
      </div>
      <div className="nav-bar__usuario">
        <span>{nombreUsuario}</span>
        <button type="button" onClick={cerrarSesion}>
          Cerrar sesión
        </button>
      </div>
    </nav>
  )
}
