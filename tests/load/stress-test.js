/**
 * PRUEBA DE ESTRÉS — Sistema Académico Integral
 * Herramienta: k6  (https://k6.io/docs/get-started/installation/)
 *
 * Instalación (Windows):
 *   winget install k6 --source winget
 *   o descargar el .msi desde https://github.com/grafana/k6/releases
 *
 * Uso:
 *   k6 run tests/load/stress-test.js                    (estrés completo)
 *   k6 run --env MODO=liviano tests/load/stress-test.js  (liviano, recomendado para empezar)
 *   k6 run --env MODO=normal  tests/load/stress-test.js  (carga normal)
 *
 * IMPORTANTE: correlo con el backend ya levantado (dotnet run en backend/src).
 */

import { scenario } from 'k6/execution';
import http from 'k6/http';
import flujoDireccion from './scenarios/flujo-direccion.js';
import flujoDocente   from './scenarios/flujo-docente.js';
import flujoPreceptor from './scenarios/flujo-preceptor.js';

// Solo 5xx y errores de red cuentan como falla — los 4xx (404, 403) son respuestas esperadas
http.setResponseCallback(http.expectedStatuses({ min: 200, max: 499 }));

// ── Perfiles de carga ────────────────────────────────────────────────────────

const MODO = __ENV.MODO ?? 'estres';

const PERFILES = {
  // Liviano: para verificar que todo funciona sin tildar la PC
  liviano: {
    scenarios: {
      direccion: { executor: 'constant-vus', vus: 3, duration: '2m', exec: 'escenarioDireccion' },
      docente:   { executor: 'constant-vus', vus: 3, duration: '2m', exec: 'escenarioDocente',   startTime: '10s' },
      preceptor: { executor: 'constant-vus', vus: 2, duration: '2m', exec: 'escenarioPreceptor', startTime: '20s' },
    },
    thresholds: {
      'http_req_duration':                  ['p(95)<5000'],
      'http_req_failed':                    ['rate<0.10'],
      'http_req_duration{scenario:reportes_pesados}': ['p(95)<8000'],
    },
  },

  // Normal: carga esperada en un día de actividad pico (inscripciones, cierre de notas)
  normal: {
    scenarios: {
      direccion: {
        executor: 'ramping-vus',
        stages:   [{ duration: '1m', target: 10 }, { duration: '3m', target: 10 }, { duration: '1m', target: 0 }],
        exec: 'escenarioDireccion',
      },
      docente: {
        executor: 'ramping-vus',
        startTime: '30s',
        stages:   [{ duration: '1m', target: 20 }, { duration: '3m', target: 20 }, { duration: '1m', target: 0 }],
        exec: 'escenarioDocente',
      },
      preceptor: {
        executor: 'ramping-vus',
        startTime: '1m',
        stages:   [{ duration: '1m', target: 10 }, { duration: '3m', target: 10 }, { duration: '1m', target: 0 }],
        exec: 'escenarioPreceptor',
      },
    },
    thresholds: {
      'http_req_duration': ['p(95)<3000', 'p(99)<8000'],
      'http_req_failed':   ['rate<0.05'],
    },
  },

  // Estrés: ver hasta dónde aguanta (100 → 150 VUs simultáneos)
  estres: {
    scenarios: {
      direccion: {
        executor: 'ramping-vus',
        stages: [
          { duration: '1m',  target: 20  },   // calentamiento
          { duration: '2m',  target: 50  },   // carga media
          { duration: '2m',  target: 100 },   // estrés
          { duration: '2m',  target: 150 },   // estrés extremo
          { duration: '1m',  target: 0   },   // enfriamiento
        ],
        exec: 'escenarioDireccion',
        tags: { tipo: 'reportes_pesados' },
      },
      docente: {
        executor: 'ramping-vus',
        startTime: '30s',
        stages: [
          { duration: '1m',  target: 30  },
          { duration: '2m',  target: 60  },
          { duration: '2m',  target: 100 },
          { duration: '2m',  target: 0   },
        ],
        exec: 'escenarioDocente',
        tags: { tipo: 'operativo' },
      },
      preceptor: {
        executor: 'ramping-vus',
        startTime: '1m',
        stages: [
          { duration: '1m',  target: 20 },
          { duration: '3m',  target: 50 },
          { duration: '2m',  target: 0  },
        ],
        exec: 'escenarioPreceptor',
        tags: { tipo: 'operativo' },
      },
    },
    thresholds: {
      // El sistema NO debe superar estos límites bajo 150 VUs
      'http_req_duration':                    ['p(95)<5000', 'p(99)<10000'],
      'http_req_failed':                      ['rate<0.10'],
      // Los reportes pesados pueden tardar más
      'http_req_duration{tipo:reportes_pesados}': ['p(95)<8000'],
      // Las operaciones diarias deben ser rápidas
      'http_req_duration{tipo:operativo}':        ['p(95)<3000'],
    },
  },
};

export const options = PERFILES[MODO] ?? PERFILES.estres;

// ── Exports de scenarios ─────────────────────────────────────────────────────

export function escenarioDireccion() { flujoDireccion(); }
export function escenarioDocente()   { flujoDocente();   }
export function escenarioPreceptor() { flujoPreceptor(); }

// ── Resumen al final ─────────────────────────────────────────────────────────

export function handleSummary(data) {
  const thresholdsFailed = Object.entries(data.metrics)
    .filter(([, m]) => m.thresholds && Object.values(m.thresholds).some(t => t.ok === false))
    .map(([name]) => name);

  const estado = thresholdsFailed.length === 0 ? '✅ PASÓ' : '❌ FALLÓ';
  const duracion = data.state.testRunDurationMs / 1000;

  console.log(`\n${'='.repeat(60)}`);
  console.log(`  RESULTADO: ${estado} (${MODO.toUpperCase()})`);
  console.log(`  Duración total: ${duracion.toFixed(0)}s`);
  console.log(`  Requests totales: ${data.metrics.http_reqs?.values?.count ?? 'N/A'}`);
  console.log(`  Tasa de error: ${((data.metrics.http_req_failed?.values?.rate ?? 0) * 100).toFixed(2)}%`);
  console.log(`  p95 respuesta: ${(data.metrics.http_req_duration?.values?.['p(95)'] ?? 0).toFixed(0)}ms`);
  console.log(`  p99 respuesta: ${(data.metrics.http_req_duration?.values?.['p(99)'] ?? 0).toFixed(0)}ms`);
  if (thresholdsFailed.length > 0) {
    console.log(`\n  Métricas que fallaron los thresholds:`);
    thresholdsFailed.forEach(m => console.log(`    - ${m}`));
  }
  console.log(`${'='.repeat(60)}\n`);

  return {};
}
