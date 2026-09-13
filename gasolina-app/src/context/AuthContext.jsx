import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import { login as loginApi } from '../api/authApi'
import { guardarToken, borrarToken, obtenerToken, setOnNoAutorizado } from '../api/httpClient'

const NOMBRE_USUARIO_KEY = 'gasolina.nombreUsuario'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => obtenerToken())
  const [nombreUsuario, setNombreUsuario] = useState(() => localStorage.getItem(NOMBRE_USUARIO_KEY))

  function cerrarSesion() {
    borrarToken()
    localStorage.removeItem(NOMBRE_USUARIO_KEY)
    setToken(null)
    setNombreUsuario(null)
  }

  useEffect(() => {
    setOnNoAutorizado(cerrarSesion)
  }, [])

  async function iniciarSesion(email, password) {
    const respuesta = await loginApi(email, password)
    guardarToken(respuesta.token)
    localStorage.setItem(NOMBRE_USUARIO_KEY, respuesta.nombreUsuario)
    setToken(respuesta.token)
    setNombreUsuario(respuesta.nombreUsuario)
  }

  const valor = useMemo(
    () => ({
      estaAutenticado: Boolean(token),
      nombreUsuario,
      iniciarSesion,
      cerrarSesion
    }),
    [token, nombreUsuario]
  )

  return <AuthContext.Provider value={valor}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const contexto = useContext(AuthContext)

  if (!contexto) {
    throw new Error('useAuth debe usarse dentro de un AuthProvider.')
  }

  return contexto
}
