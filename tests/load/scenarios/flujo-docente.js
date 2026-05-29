/**
 * Flujo de Docente — carga de notas y reportes propios.
 * Simula a un docente registrando asistencias y consultando su rendimiento.
 */
import http from 'k6/http';
import { check, sleep } from 'k6';
import { login } from '../helpers/auth.js';
import { USUARIOS, BASE_URL, authHeader, PARAMS_JSON } from '../helpers/config.js';

export default function flujoDocente() {
  // Si no tenés usuario docente configurado, usa direccion como fallback
  const creds = USUARIOS.docente ?? USUARIOS.direccion;
  const token = login(creds.email, creds.password);
  if (!token) return;

  const h = authHeader(token);

  // 1. Listar exámenes disponibles
  let res = http.get(`${BASE_URL}/api/examenes`, h);
  check(res, { 'examenes → 200': r => r.status === 200 });
  sleep(1);

  // 2. Reporte de evolución de notas de sus materias
  res = http.get(`${BASE_URL}/api/reportes/rendimiento/evolucion`, h);
  check(res, { 'evolucion → 200': r => r.status === 200 });
  sleep(1.5);

  // 3. Promedios de sus cátedras
  res = http.get(`${BASE_URL}/api/reportes/rendimiento/catedras`, h);
  check(res, { 'catedras-docente → 200': r => r.status === 200 });
  sleep(1);

  // 4. Reporte de inasistencias de sus materias
  res = http.post(
    `${BASE_URL}/api/reportes/inasistencias`,
    JSON.stringify({ soloAusencias: true }),
    { headers: { ...h.headers } },
  );
  check(res, { 'inasistencias-docente → 200': r => r.status === 200 });
  sleep(2);

  // 5. Comparativo de comisiones
  res = http.get(`${BASE_URL}/api/reportes/rendimiento/comisiones`, h);
  check(res, { 'comisiones → 200': r => r.status === 200 });
  sleep(2);
}
