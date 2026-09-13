import { Outlet } from 'react-router-dom'
import NavBar from './NavBar'

export default function Layout() {
  return (
    <div className="layout">
      <NavBar />
      <main className="layout__contenido">
        <Outlet />
      </main>
    </div>
  )
}
