import React, { useState, useEffect } from 'react';
import axios from 'axios';

export default function Dashboard({ user, onLogout }) {
  const [empresa, setEmpresa] = useState('TRANS_NNU');
  const [empleados, setEmpleados] = useState([]);
  const [cargando, setCargando] = useState(false);

  useEffect(() => {
    cargarEmpleados();
  }, [empresa]);

  const cargarEmpleados = async () => {
    setCargando(true);
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get(`http://localhost:5000/api/empleados?empresa=${empresa}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      setEmpleados(response.data);
    } catch (err) {
      alert('Error al cargar la lista de empleados');
    } finally {
      setCargando(false);
    }
  };

  return (
    <div style={{ padding: '20px' }}>
      <header style={{ display: 'flex', justifyContent: 'space-between', borderBottom: '1px solid #ccc', pb: '10px' }}>
        <div>
          <h1>SINAITVWEB - Control de Asistencia</h1>
          <p>Usuario: <strong>{user.nombreUsuario}</strong> | Rol: <span style={{ color: 'blue' }}>{user.rol}</span></p>
        </div>
        <button onClick={onLogout} style={{ height: '40px', marginTop: '20px' }}>Cerrar Sesión</button>
      </header>

      <div style={{ marginTop: '20px' }}>
        <label>Seleccionar Empresa: </label>
        <select value={empresa} onChange={(e) => setEmpresa(e.target.value)} style={{ padding: '5px', marginLeft: '10px' }}>
          <option value="TRANS_NNU">INVERSIONES TRANSVICTORIA, C.A</option>
          <option value="SERPRA_N">INVERSIONES SERPRAGECA C.A.</option>
        </select>
      </div>

      <h3 style={{ marginTop: '20px' }}>Listado de Personal</h3>
      {cargando ? (
        <p>Cargando datos...</p>
      ) : (
        <table border="1" cellPadding="8" cellSpacing="0" style={{ width: '100%', marginTop: '10px' }}>
          <thead>
            <tr style={{ backgroundColor: '#f2f2f2' }}>
              <th>Cédula</th>
              <th>Código Emp</th>
              <th>Nombre Completo</th>
              <th>ID Biométrico</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {empleados.map((emp) => (
              <tr key={emp.codEmp}>
                <td>{emp.cedula}</td>
                <td>{emp.codEmp}</td>
                <td>{emp.nombreCompleto}</td>
                <td>{emp.idBiometrico || 'Sin Mapear'}</td>
                <td>
                  {user.rol === 'SoloLectura' ? (
                    <span style={{ color: 'gray' }}>Solo Lectura</span>
                  ) : (
                    <button style={{ backgroundColor: '#28a745', color: '#fff', border: 'none', padding: '5px 10px', borderRadius: '4px' }}>
                      + Justificación / Mapeo
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}