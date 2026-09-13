import axios from 'axios'

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5114'
const TOKEN_KEY = 'gasolina.token'

let vistaActual = 'Desconocida'
let onNoAutorizado = null

export function setVistaOrigen(nombreVista) {
  vistaActual = nombreVista
}

export function setOnNoAutorizado(callback) {
  onNoAutorizado = callback
}

export function guardarToken(token) {
  localStorage.setItem(TOKEN_KEY, token)
}

export function obtenerToken() {
  return localStorage.getItem(TOKEN_KEY)
}

export function borrarToken() {
  localStorage.removeItem(TOKEN_KEY)
}

export const httpClient = axios.create({
  baseURL: BASE_URL
})

httpClient.interceptors.request.use((config) => {
  const token = obtenerToken()

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  config.headers['X-Vista-Origen'] = vistaActual

  return config
})

httpClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      borrarToken()
      onNoAutorizado?.()
    }

    return Promise.reject(error)
  }
)
