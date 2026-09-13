import { httpClient } from './httpClient'

export async function login(email, password) {
  const { data } = await httpClient.post('/api/auth/login', { email, password })
  return data.datos
}
