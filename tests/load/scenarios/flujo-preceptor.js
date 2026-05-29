/**
 * Flujo de Preceptor — control de asistencia y legajos individuales.
 * Simula a un preceptor revisando alumnos en riesgo y controlando inasistencias.
 */
import http from 'k6/http';
import { check, sleep } from 'k6';
import { login } from '../helpers/auth.js';
import { USUARIOS, BASE_URL, authHeader } from '../helpers/config.js';

// Legajos de ejemplo — reemplazá con legajos reales de tu DB
const LEGAJOS_PRUEBA = ['2024-001', '2024-002', '2024-003', '2023-001', '2023-002'];

export default function flujoPreceptor() {
  const creds = USUARIOS.preceptor ?? USUARIOS.direccion;
  const token = login(creds.email, creds.password);
  if (!token) return;

  const h = authHeader(token);

  // 1. Control individual de un legajo aleatorio
  const legajo = LEGAJOS_PRUEBA[Math.floor(Math.random() * LEGAJOS_PRUEBA.length)];
  let res = http.get(`${BASE_URL}/api/reportes/control-legajo/${encodeURIComponent(legajo)}`, h);
  // 404 es esperado si el legajo no existe en la DB — solo validamos que no crashee
  check(res, { 'control-legajo → no crash': r => r.status !== 500 });
  sleep(1.5);

  // 2. Reporte de inasistencias con filtro de fecha reciente
  res = http.post(
    `${BASE_URL}/api/reportes/inasistencias`,
    JSON.stringify({
      soloAusencias:  true,
      fechaDesde:     '2025-01-01',
      fechaHasta:     '2025-12-31',
    }),
    { headers: { ...h.headers } },
  );
  check(res, { 'inasistencias-preceptor → 200': r => r.status === 200 });
  sleep(2);

  // 3. Listado de inscripciones
  res = http.get(`${BASE_URL}/api/inscripciones`, h);
  check(res, { 'inscripciones → no crash': r => r.status !== 500 });
  sleep(1);

  // 4. Listado de cursos
  res = http.get(`${BASE_URL}/api/cursos`, h);
  check(res, { 'cursos → 200 o 403': r => [200, 403].includes(r.status) });
  sleep(1.5);
}
