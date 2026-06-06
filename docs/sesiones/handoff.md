# Handoff — Coherencia de Base de Datos

## Objetivo General

Lograr que todos los datos del sistema sean estadísticamente realistas y mutuamente coherentes, de modo que los reportes del tablero ejecutivo y los módulos de análisis reflejen una realidad académica creíble.

El problema raíz era que los seeders creaban todos los alumnos con `Condicion = Regular` y asistencias/notas/historial uniformes, sin distinguir entre condiciones académicas.

---

## Distribución Target (activos, excluyendo Egresados y Desertores)

| Condición | Rango | Descripción |
|---|---|---|
| Regular | 55–65% | Aprobaron parciales y prácticos (nota ≥ 4) pero deben rendir final |
| Libre | 20–30% | No cumplieron asistencia, reprobaron parciales, o rinden sin cursar |
| Promocional | 10–20% | Promedio ≥ 7 en todas las instancias, sin aplazos |

Las distribuciones oscilan entre cohortes (no son idénticas), lo que da naturalidad al dataset.

---

## Lo Que Se Hizo (Completado)

### 1. `Estudiante.Condicion` ✅

Se actualizó directamente vía SQL por cohorte con distribuciones que oscilan dentro del rango target:

| Cohorte | Regular | Libre | Promo |
|---|---|---|---|
| 2021 | 29 (61.7%) | 11 (23.4%) | 7 (14.9%) |
| 2022 | 37 (58.7%) | 17 (27.0%) | 9 (14.3%) |
| 2023 | 36 (60.0%) | 13 (21.7%) | 11 (18.3%) |
| 2024 | 39 (57.4%) | 20 (29.4%) | 9 (13.2%) |

**Método:** reset a Regular → TOP N por Id ASC para Libre → TOP N por Id DESC para Promocional.

El `CondicionRealistaSeeder` (C#) detecta `Libre ≥ 20%` y se salta automáticamente al reiniciar el backend — no pisa estos cambios.

---

### 2. `Asistencias` ✅

Se alineó la asistencia con la condición académica de cada estudiante:

| Condición | Asistencia resultante |
|---|---|
| Libre | ~32–47% (dejaron de asistir a mitad de año) |
| Regular | 54–72% (sin cambios) |
| Promocional | 87–94% (alta asistencia, pocas faltas) |

**Método:**
- Libre → convertir el último 40% de registros `Presente` a `Ausente` (ordenados por fecha ASC por estudiante)
- Promocional → convertir 85% de los registros `Ausente` a `Presente` (ordenados por fecha ASC por estudiante)

---

### 3. `HistorialAcademico` ✅

Se corrigieron cuatro inconsistencias:

- **3a:** Libres con registros `Condicion='Promocional'` (409 registros) → bajados a `Regularizado / Regular / nota 4–6`
- **3b:** Promocionales con registros `Condicion='Libre'` (181 registros) → subidos a `Regularizado / Regular / nota 5–6`
- **3c:** Año más reciente de cada Libre → `EstadoFinal='Libre' / nota 1–3 / Condicion='Libre'`
- **3d:** Año más reciente de cada Promo → `EstadoFinal='Promocional' / nota 8–10 / Condicion='Promocional'`

**Resultado final:**
- Libre: `EstadoFinal='Libre'` con nota promedio ~2.0
- Promo: `EstadoFinal='Promocional'` con nota promedio ~9.0
- Los registros históricos de años anteriores conservan condiciones previas (realista)

---

### 4. `InscripcionesExamen` ✅

Se alinearon notas y estados de exámenes con la condición actual:

| Condición | Antes | Después |
|---|---|---|
| Libre | 75% Aprobada, nota avg 6.87 | 22% Aprobada (nota 4–6), 78% Desaprobada (nota 1–3) |
| Promocional | 65% Aprobada, nota avg 5.69 | 94% Aprobada (nota 7–10), 4.5% Desaprobada |
| Regular | sin cambios | sin cambios |

---

### 5. `InscripcionMateria` ✅

Se alineó el estado de cursado con la condición:

| Condición | Antes | Después |
|---|---|---|
| Libre | 83% Aprobada, 17% Desaprobada | 23% Aprobada, 77% Desaprobada |
| Promocional | 70% Aprobada, 30% Desaprobada | 96.5% Aprobada, 3.5% Desaprobada |
| Regular | sin cambios | sin cambios |

---

### 6. Correcciones de Frontend y Backend ✅

Fixes realizados en esta sesión sobre los reportes del tablero:

- **Color Regulares:** la card de condición pasó de amarillo (`medio`) a azul (`azul`) para coincidir con el color del gráfico donut.
- **Label "Matriculados en el año":** la columna "Total Activos" en la tabla de evolución de matrícula fue renombrada para reflejar correctamente que cuenta actividad histórica en ese año, no el estado actual.
- **Promedio ponderado de duración de egreso:** `EgresadosPorCarreraUseCase` calculaba el promedio global como promedio simple de promedios de cohorte. Corregido a promedio ponderado por cantidad de egresados (resultado: 2.2 años en vez de 2.4).
- **Fechas de egreso futuras:** el Trayecto 2024 tenía `FechaDeEgreso` en rango Oct 2026–Mar 2027 (futuro). Se agregó `CohorteHistoricaSeeder.CapFechasEgresoFuturaAsync` que las corrige a Oct 2025–May 2026 al arrancar el backend.
- **Seeders deshabilitados:** todos los seeders de datos fueron comentados en `Program.cs` para evitar que el backend pise cambios manuales de SQL al reiniciarse. Solo quedan activos la creación de admin y carreras (ya protegidos por guards).

---

## Estado del Backend

- El backend arranca sin errores de compilación.
- Ningún seeder de datos se ejecuta al reiniciar — los datos son seguros.
- Los `warn` de `CorrelativiadadesSeeder` sobre materias no encontradas son conocidos y no son bloqueantes.

---

## Pendiente

### Migración de años +2 (en curso)

**Objetivo:** desplazar todos los registros académicos 2 años hacia adelante para que las cohortes queden en el rango 2023–2026 en lugar de 2021–2024. Esto da más realismo temporal al dataset (el sistema parece operar en el presente).

**Regla:** `2021 → 2023 · 2022 → 2024 · 2023 → 2025 · 2024 → 2026`

**Diagnóstico inicial relevado:**

| Tabla | Campo | Rango actual | Registros |
|---|---|---|---|
| `Estudiantes` | `FechaDeIngreso` | 2021–2024 | 480 |
| `Estudiantes` | `FechaDeEgreso` | 2022–2026 | 79 |
| `HistorialAcademico` | `Anio` | 2021–2024 | 6.294 |
| `Cursos` | `Anio` | 2021–2026 | 58 |
| `Asistencias` | `Fecha` | 2021–2024 | 231.881 |
| `InscripcionesMateria` | `FechaInscripcion` | 2021–2024 | 6.420 |
| `Examenes` | `FechaExamen` | 2021–2024 | 536 |
| `InscripcionesExamen` | `FechaInscripcion` | 2021–2026 | 22.307 |
| `RespuestasEncuesta` | `Fecha` | 2021–2024 | 3.651 |

**Tablas que NO se tocan:** `CalendarioAcademico` y `EncuestasCompletadas` (ya están en 2026, son datos actuales correctos).

**Plan de ejecución — 9 pasos SQL (uno por tabla, con verificación):**

| Paso | Tabla | Estado |
|---|---|---|
| 1 | `Estudiantes.FechaDeIngreso` | ⬜ Pendiente |
| 2 | `Estudiantes.FechaDeEgreso` | ⬜ Pendiente |
| 3 | `HistorialAcademico.Anio` | ⬜ Pendiente |
| 4 | `Cursos.Anio` | ⬜ Pendiente |
| 5 | `Asistencias.Fecha` | ⬜ Pendiente |
| 6 | `InscripcionesMateria.FechaInscripcion` | ⬜ Pendiente |
| 7 | `Examenes.FechaExamen` | ⬜ Pendiente |
| 8 | `InscripcionesExamen.FechaInscripcion` | ⬜ Pendiente |
| 9 | `RespuestasEncuesta.Fecha` | ⬜ Pendiente |

Después de completar la migración: verificar coherencia de los reportes del tablero ejecutivo (distribución de egresados por cohorte, evolución de matrícula, promedios por cátedra).

### Coherencia en los Reportes (post-migración)

| Reporte | Qué verificar |
|---|---|
| **Tablero Ejecutivo** | Donut de condición muestra ~58% Regular · ~25% Libre · ~15% Promo |
| **Retención por Cohorte** | Curvas de retención decrecientes y realistas por año |
| **Deserción por Año** | Refleja mayor deserción en cohortes más antiguas |
| **Riesgo Académico** | Libre aparece como grupo en riesgo con baja asistencia y notas |
| **Comparativo Comisiones** | Notas promedio coherentes: Libre < 4, Regular 4–6, Promo 7–10 |
| **Evolución de Notas** | Tendencia temporal coherente con los seeders por año |
| **Promedios por Cátedra** | Promedios razonables (no todos 5.0 uniformes) |
| **Egresados por Carrera** | Distribución coherente: cohortes más antiguas con mayor tasa de egreso |
