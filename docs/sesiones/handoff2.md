# Handoff 2 — Reporte de Deserción por Año

**Fecha:** 2026-06-05  
**Rama:** `main`

---

## Objetivo de la sesión

Analizar por qué el **reporte "Deserción por Año de Cursada"** muestra **0% de deserción en ciertos años y cohortes**, e implementar una corrección de datos que lo haga coherente.

---

## Problema identificado

El reporte tiene **dos problemas separados**:

### Problema A — Datos de seed: estudiantes "fantasma" (EST-D)

`CohorteHistoricaSeeder.SeedDesertoresActivosAsync` creó desertores históricos sin ningún registro académico (sin inscripciones de materia, sin asistencias, sin historial). Estos estudiantes:
- **SÍ aparecen en el numerador** del reporte (son `Condicion = Desertor`)
- **NO aparecen en el denominador** (que se basa en `InscripcionesMateria`)

Resultado: tasas de deserción artificialmente infladas y 0% en cohortes donde los phantoms son los únicos "desertores" contabilizados.

Prefijos de fantasmas creados:
- `EST-D2023-C1` → Prof 2023 Año 1 (55), Año 2 (22), Año 3 (13)
- `EST-D2024-C1` → Prof 2024 Año 1 (65), Año 2 (25)
- `EST-D2025-C1` → Prof 2025 Año 1 (49)
- `EST-D2025-C2` → Trayecto 2025 Año 1 (29)

### Problema B — Ningún seeder genera desertores con `Estudiante.Anio = 4`

La query del reporte cruza `Curso.AnioLectivo` (denominador, 1-4) con `Estudiante.Anio` (numerador, 1-4). Ningún seeder histórico crea desertores de Profesorado en año 4:

| Seeder | Desertores generados |
|--------|----------------------|
| `CohorteHistoricaSeeder` (Prof 2021/2022) | `DesertoresPorAnio: [10, 4, 3]` → solo años 1, 2, 3 |
| `SeedDesertoresActivosAsync` (Prof 2023) | `[(1,55),(2,22),(3,13)]` → solo años 1, 2, 3 |
| `SeedDesertoresActivosAsync` (Prof 2024) | `[(1,65),(2,25)]` → solo años 1, 2 |

### Problema C — Cohortes recientes creadas 100% como Regular

- `NuevosEstudiantes2025Seeder` → 60 alumnos todos `Regular`
- `NuevosEstudiantes2026Seeder` → 60 alumnos todos `Regular`

Sus desertores se generaban como fantasmas (sin registros), que es justamente el Problema A.

---

## Casos con Deserción = 0 identificados

### Con filtro de cohorte específica

| Cohorte | Año | Causa |
|---------|-----|-------|
| Prof 2021 | Año 4 | Ningún seeder pone desertores con `Anio=4` |
| Prof 2022 | Año 4 | Ídem |
| Prof 2023 | Año 4 | Ídem; recién cursan 4° en 2026 |
| Prof 2024 | Años 3 y 4 | Solo cursaron 1° y 2°, no hay datos de años posteriores |
| Prof 2025 | Año 1 | 49 phantoms EST-D fuera del denominador; 60 Regulares sin ningún desertor real |
| Prof 2025 | Año 2 | No cursaron 2° todavía |
| Prof 2026 | Años 1-4 | 60 Regulares, cero desertores |
| Trayecto 2025 | Año 2 | EST-D phantoms + `NuevosEstudiantes2025TrayectoSeeder` ya tiene distribución propia (ver nota) |
| Trayecto 2026 | Año 1 | Solo 3 desertores por comisión (6 total, podría estar bien) |

### Sin filtro (todas las cohortes combinadas)

| Año | Causa |
|-----|-------|
| Año 4 Profesorado | Siempre 0 — ningún seeder asignó `Anio=4` a un `Desertor` en toda la BD |

---

## Arquitectura de la query (para entenderla)

**Archivo:** [RendimientoConsolidadoRepository.cs](backend/src/Infrastructure/Persistence/Repositories/RendimientoConsolidadoRepository.cs) — método `ObtenerDesercionPorAnioAsync` (~línea 510)

```
Denominador: InscripcionesMateria → join Estudiantes → join Cursos
             Agrupa por Curso.AnioLectivo (1-4)
             Filtra por CarreraId y FechaDeIngreso.Year si aplica

Numerador:   Estudiantes WHERE Condicion = Desertor
             Agrupa por Estudiante.Anio (1-4)

Join final:  desertoresPorAnio.GetValueOrDefault(AnioLectivo, 0)
             → AnioLectivo y Estudiante.Anio ambos 1-4 → deben coincidir
```

---

## Solución implementada

### Archivo creado: `PatchDesercionSeeder.cs`

**Ruta:** [backend/src/Infrastructure/Seeding/PatchDesercionSeeder.cs](backend/src/Infrastructure/Seeding/PatchDesercionSeeder.cs)

**Idempotente:** Guard por `Estudiantes WHERE CarreraId=1 AND FechaDeIngreso.Year=2025 AND Condicion=Desertor`.

#### Paso 1 — Eliminar fantasmas EST-D
Borra de `Usuarios`, `Estudiantes` (y por las dudas: `HistorialAcademico`, `InscripcionesMateria`, `Asistencias`) los prefijos `EST-D2023-C1`, `EST-D2024-C1`, `EST-D2025-C1`, `EST-D2025-C2`.

#### Paso 2 — Prof 2025: 20 desertores reales sobre los 60 existentes
Los primeros 20 Regulares (orden por Id) se convierten:
- **14 "deserción temprana"**: borra asistencias desde `2025-07-01` en adelante, elimina todas sus `InscripcionesExamen`, historial → "Abandonó" sin nota, `InscripcionMateria.Estado = Baja`, `Condicion = Desertor Anio=1`
- **6 "deserción por riesgo"**: asistencias desde `2025-06-01` → todas `Ausente`, notas de parciales → `(Id % 3) + 1` (1-3), elimina inscripción al examen `Final`, historial → "Abandonó", `Baja`, `Desertor Anio=1`

#### Paso 3 — Prof 2026: 8 desertores tempranos
Borra asistencias desde `2026-05-01`, elimina `InscripcionesExamen`, historial "Abandonó", `Baja`, `Desertor Anio=1`. Representa ~13% de la cohorte (coherente con el año en curso).

#### Paso 4 — Año 4 histórico: Prof 2021 y 2022
Toma los últimos 2 Regulares con `Anio=4` de cada cohorte y les cambia `Condicion = Desertor, Anio = 4`. Mantienen todos sus registros intactos (abandonaron al final del recorrido).

### Registro en `Program.cs`

```csharp
await PatchDesercionSeeder.PatchAsync(db, logger);   // activo, sin comentar
```

Está ubicado en el bloque `-- Correcciones / patches --`, antes del `PatchEgresadosSeeder`.

---

## Resultado esperado después del patch

| Cohorte / Año | Antes | Después |
|---|---|---|
| Prof 2025 / Año 1 | 0% | ~33% (20/60) |
| Prof 2026 / Año 1 | 0% | ~13% (8/60) |
| Prof 2021 / Año 4 | 0% | ~13% (2 de ~15 activos) |
| Prof 2022 / Año 4 | 0% | ~13% (2 de ~15 activos) |

---

## Archivos leídos en esta sesión

### Frontend
- [frontend/src/app/features/reportes/panel-desercion-anio/panel-desercion-anio.component.ts](frontend/src/app/features/reportes/panel-desercion-anio/panel-desercion-anio.component.ts)
- [frontend/src/app/features/reportes/panel-desercion-anio/panel-desercion-anio.component.html](frontend/src/app/features/reportes/panel-desercion-anio/panel-desercion-anio.component.html)

### Backend — Application
- [backend/src/Application/Reportes/DesercionPorAnioUseCase.cs](backend/src/Application/Reportes/DesercionPorAnioUseCase.cs)
- [backend/src/Application/Reportes/DTOs/DesercionPorAnioDto.cs](backend/src/Application/Reportes/DTOs/DesercionPorAnioDto.cs)

### Backend — Domain
- [backend/src/Domain/Entities/Estudiante.cs](backend/src/Domain/Entities/Estudiante.cs)
- [backend/src/Domain/Entities/Curso.cs](backend/src/Domain/Entities/Curso.cs)
- [backend/src/Domain/Entities/HistorialAcademico.cs](backend/src/Domain/Entities/HistorialAcademico.cs)
- [backend/src/Domain/Enums/EstadoInscripcion.cs](backend/src/Domain/Enums/EstadoInscripcion.cs)

### Backend — Infrastructure (Repository)
- [backend/src/Infrastructure/Persistence/Repositories/RendimientoConsolidadoRepository.cs](backend/src/Infrastructure/Persistence/Repositories/RendimientoConsolidadoRepository.cs) — método `ObtenerDesercionPorAnioAsync` en línea ~510

### Backend — Seeders leídos
| Archivo | Por qué se leyó |
|---------|-----------------|
| [CursosSeeder.cs](backend/src/Infrastructure/Seeding/CursosSeeder.cs) | Entender `Curso.Anio` vs `Curso.AnioLectivo` |
| [CohorteHistoricaSeeder.cs](backend/src/Infrastructure/Seeding/CohorteHistoricaSeeder.cs) | Origen de desertores históricos y fantasmas EST-D |
| [Anio2024ActividadesSeeder.cs](backend/src/Infrastructure/Seeding/Anio2024ActividadesSeeder.cs) — primeras 80 líneas | Patrón de actividades y desertores en año 2024 |
| [Anio2025ActividadesSeeder.cs](backend/src/Infrastructure/Seeding/Anio2025ActividadesSeeder.cs) | Patrón completo, `CondicionEfectiva`, `MarcarDesertores` |
| [Anio2026ActividadesSeeder.cs](backend/src/Infrastructure/Seeding/Anio2026ActividadesSeeder.cs) | Igual que 2025 para el año en curso |
| [NuevosEstudiantes2025Seeder.cs](backend/src/Infrastructure/Seeding/NuevosEstudiantes2025Seeder.cs) | Confirmó: 60 alumnos todos Regular |
| [NuevosEstudiantes2025TrayectoSeeder.cs](backend/src/Infrastructure/Seeding/NuevosEstudiantes2025TrayectoSeeder.cs) | Modelo correcto con distribución de desertores |
| [NuevosEstudiantes2026Seeder.cs](backend/src/Infrastructure/Seeding/NuevosEstudiantes2026Seeder.cs) | Confirmó: 60 alumnos todos Regular |
| [NuevosEstudiantes2026TrayectoSeeder.cs](backend/src/Infrastructure/Seeding/NuevosEstudiantes2026TrayectoSeeder.cs) | Ya tiene 3 desertores Y1 por comisión |
| [PatchEgresadosSeeder.cs](backend/src/Infrastructure/Seeding/PatchEgresadosSeeder.cs) | Patrón de patch para copiar |
| [Program.cs](backend/src/Program.cs) — sección seeders | Orden de ejecución y qué está activo/comentado |

---

## Contexto de datos de seed (resumen)

### Modelo de cursos
- `Curso.Anio` = año calendárico (2021, 2022, ..., 2026)
- `Curso.AnioLectivo` = año del plan de estudios (1, 2, 3, 4 para Profesorado; 1, 2 para Trayecto)

### Distribución de cohortes (Profesorado, CarreraId=1)

| Cohorte | Situación antes del patch |
|---------|--------------------------|
| 2021 | 10E+17D(A1-A3)+8R × 2 comisiones; egresados corregidos por PatchEgresadosSeeder |
| 2022 | Similar a 2021 |
| 2023 | 90 activos (del seeder) + 90 fantasmas EST-D |
| 2024 | ~61 activos + 90 fantasmas EST-D; 8 desertores reales A2 en 2025 |
| 2025 | 60 Regular + 49 fantasmas EST-D (sin registros) |
| 2026 | 60 Regular + 0 desertores |

### Distribución de cohortes (Trayecto, CarreraId=2)

| Cohorte | Situación antes del patch |
|---------|--------------------------|
| 2021-2024 | Correctas; `NuevosEstudiantes*TrayectoSeeder` con distribución propia |
| 2025 | 20D+6D(A2)+34R (TrayectoSeeder) + 29 fantasmas EST-D redundantes |
| 2026 | 3D+27R por comisión (ya correcto en seeder) |

---

## Lo que NO se hizo (posibles mejoras futuras)

1. **Modificar los seeders base** (`NuevosEstudiantes2025Seeder`, `NuevosEstudiantes2026Seeder`) para que creen la distribución correcta desde el origen. El patch actual funciona sobre datos ya existentes en la BD.

2. **Trayecto 2025 — eliminar los 29 phantoms EST-D2025-C2**: el `PatchDesercionSeeder` SÍ los elimina (incluido en `EliminarFantasmas`). Pero hay que verificar que el `NuevosEstudiantes2025TrayectoSeeder` ya cubre correctamente la distribución esperada para que el reporte sea coherente también para Trayecto 2025.

3. **Agregar año 4 a `CohorteHistoricaSeeder.Grupos`**: la forma "limpia" sería modificar `Grupos` para incluir desertores de año 4. Requiere también actualizar el guard en `RepararAsync` (actualmente checkea `desertoresC1_2021 <= 34`).

4. **Prof 2023 y 2024 Año 4**: cuando esas cohortes lleguen al año 4 (2026 para 2023, 2027 para 2024), habrá que agregar más desertores.

---

## Cómo verificar que el patch funcionó

1. Reiniciar el backend → el seeder se ejecuta al inicio.
2. Ir al reporte "Deserción por Año de Cursada".
3. Filtrar por cohorte 2025 → debe mostrar Año 1 con ~33%.
4. Filtrar por cohorte 2026 → debe mostrar Año 1 con ~13%.
5. Sin filtro → Año 4 debe mostrar valores > 0%.
6. Filtrar por cohorte 2021 o 2022 → Año 4 debe mostrar valores > 0%.

---

## Estado del repositorio al cerrar la sesión

```
M  .claude/settings.json
M  backend/src/Infrastructure/Persistence/Repositories/RendimientoConsolidadoRepository.cs
M  backend/src/Infrastructure/Seeding/CohorteHistoricaSeeder.cs
M  backend/src/Program.cs                            ← patch registrado aquí
M  frontend/src/app/features/reportes/tablero-ejecutivo/tablero-ejecutivo.component.html
?? backend/src/Infrastructure/Seeding/Anio2025ActividadesSeeder.cs
?? backend/src/Infrastructure/Seeding/Anio2026ActividadesSeeder.cs
?? backend/src/Infrastructure/Seeding/NuevosEstudiantes2025Seeder.cs
?? backend/src/Infrastructure/Seeding/NuevosEstudiantes2025TrayectoSeeder.cs
?? backend/src/Infrastructure/Seeding/NuevosEstudiantes2026Seeder.cs
?? backend/src/Infrastructure/Seeding/NuevosEstudiantes2026TrayectoSeeder.cs
?? backend/src/Infrastructure/Seeding/PatchEgresadosSeeder.cs
?? backend/src/Infrastructure/Seeding/PatchDesercionSeeder.cs  ← NUEVO
```

El único archivo nuevo de esta sesión es `PatchDesercionSeeder.cs` y la línea en `Program.cs`.
