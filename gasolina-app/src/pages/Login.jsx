import { useEffect, useState } from 'react'
import { Navigate, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { setVistaOrigen } from '../api/httpClient'

export default function Login() {
  const { estaAutenticado, iniciarSesion } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState(null)
  const [cargando, setCargando] = useState(false)

  useEffect(() => {
    setVistaOrigen('Login')
  }, [])

  if (estaAutenticado) {
    return <Navigate to="/dashboard" replace />
  }

  async function manejarEnvio(evento) {
    evento.preventDefault()
    setError(null)
    setCargando(true)

    try {
      await iniciarSesion(email, password)
      navigate('/dashboard', { replace: true })
    } catch (error) {
      setError(error.response?.data?.mensaje ?? 'No se pudo iniciar sesión.')
    } finally {
      setCargando(false)
    }
  }

  return (
    <div className="min-h-svh flex items-center justify-center p-4">
      <form
        className="bg-superficie border border-borde rounded-xl p-8 w-full max-w-[360px] flex flex-col gap-3.5"
        onSubmit={manejarEnvio}
      >
        <h1>Control de Gasolina</h1>

        <label>
          Email
          <input
            type="email"
            value={email}
            onChange={(evento) => setEmail(evento.target.value)}
            required
          />
        </label>

        <label>
          Contraseña
          <input
            type="password"
            value={password}
            onChange={(evento) => setPassword(evento.target.value)}
            required
          />
        </label>

        {error && <p className="mensaje-error">{error}</p>}

        <button type="submit" disabled={cargando}>
          {cargando ? 'Ingresando...' : 'Ingresar'}
        </button>
      </form>
    </div>
  )
}
