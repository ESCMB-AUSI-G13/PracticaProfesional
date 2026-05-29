export const BASE_URL = 'http://localhost:5000';

// Credenciales de prueba — creá estos usuarios en la DB antes de correr las pruebas
// o usá el admin para todos los flujos en primera instancia
export const USUARIOS = {
  direccion: {
    email:    'admin@institucion.edu.ar',
    password: 'Admin1234!',
  },
  // Agregá un docente y preceptor reales para pruebas multi-rol:
  // docente: { email: 'docente@institucion.edu.ar', password: 'Docente123!' },
  // preceptor: { email: 'preceptor@institucion.edu.ar', password: 'Preceptor123!' },
};

// Parámetros HTTP comunes
export const PARAMS_JSON = {
  headers: { 'Content-Type': 'application/json' },
};

export function authHeader(token) {
  return {
    headers: {
      'Content-Type':  'application/json',
      'Authorization': `Bearer ${token}`,
    },
  };
}
