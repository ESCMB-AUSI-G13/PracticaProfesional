/**
 * Flujo de Dirección — reportes pesados e institucionales.
 * Simula a un director navegando el tablero y generando reportes.
 */
import http from 'k6/http';
import { check, sleep } from 'k6';
import { login } from '../helpers/auth.js';
import { USUARIOS, BASE_URL, authHeader } from '../helpers/config.js';

export default function flujoDireccion() {
  const token = login(USUARIOS.direccion.email, USUARIOS.direccion.password);
  if (!token) return;

  const h = authHeader(token);

  // 1. Tablero ejecutivo
  let res = http.get(`${BASE_URL}/api/reportes/tablero-ejecutivo`, h);
  check(res, { 'tablero → 200': r => r.status === 200 });
  sleep(1.5);

  // 2. Riesgo académico
  res = http.get(`${BASE_URL}/api/reportes/riesgo-academico`, h);
  check(res, { 'riesgo → 200': r => r.status === 200 });
  sleep(1);

  // 3. Retención por cohorte
  res = http.get(`${BASE_URL}/api/reportes/retencion-cohorte`, h);
  check(res, { 'retencion-cohorte → 200': r => r.status === 200 });
  sleep(1);

  // 4. Retención anual
  res = http.get(`${BASE_URL}/api/reportes/retencion-anual`, h);
  check(res, { 'retencion-anual → 200': r => r.status === 200 });
  sleep(2);

  // 5. Promedios por cátedra
  res = http.get(`${BASE_URL}/api/reportes/rendimiento/catedras`, h);
  check(res, { 'catedras → 200': r => r.status === 200 });
  sleep(1.5);

  // 6. Descarga PDF del tablero (endpoint más costoso)
  res = http.get(`${BASE_URL}/api/reportes/tablero-ejecutivo/pdf`, h);
  check(res, {
    'tablero PDF → 200':   r => r.status === 200,
    'tablero PDF → es PDF': r => (r.headers['Content-Type'] ?? '').includes('pdf'),
  });
  sleep(3);
}
