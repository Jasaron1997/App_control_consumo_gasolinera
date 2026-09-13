import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import RutaProtegida from './components/RutaProtegida'
import Layout from './components/Layout'
import Login from './pages/Login'
import Dashboard from './pages/Dashboard'
import RegistrarCarga from './pages/RegistrarCarga'
import Historial from './pages/Historial'
import Vehiculos from './pages/Vehiculos'
import './App.css'

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />

          <Route
            element={
              <RutaProtegida>
                <Layout />
              </RutaProtegida>
            }
          >
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/registrar-carga" element={<RegistrarCarga />} />
            <Route path="/historial" element={<Historial />} />
            <Route path="/vehiculos" element={<Vehiculos />} />
          </Route>

          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
