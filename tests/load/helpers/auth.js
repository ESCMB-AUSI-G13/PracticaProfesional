import http from 'k6/http';
import { check } from 'k6';
import { BASE_URL, PARAMS_JSON } from './config.js';

/**
 * Hace login y devuelve el token JWT.
 * Retorna null si el login falla (el VU se saltea el test).
 */
export function login(email, password) {
  const res = http.post(
    `${BASE_URL}/api/auth/login`,
    JSON.stringify({ email, password }),
    PARAMS_JSON,
  );

  const ok = check(res, {
    'login → 200': r => r.status === 200,
    'login → tiene token': r => {
      try { return !!JSON.parse(r.body).token; } catch { return false; }
    },
  });

  if (!ok) return null;
  return JSON.parse(res.body).token;
}
