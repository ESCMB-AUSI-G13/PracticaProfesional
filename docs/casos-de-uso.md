# Casos de Uso Implementados

## Módulo 1 — Gestión de Usuarios y Roles (CU-02)

| Caso de uso | Estado | Roles habilitados |
|---|---|---|
| Crear / modificar / desactivar Docente | Implementado | Administrador |
| Crear / modificar / desactivar Preceptor | Implementado | Administrador |
| Crear / modificar / desactivar Estudiante | Implementado | Administrador, Preceptor |
| Gestión de usuarios del sistema | Implementado | Administrador |
| Recuperación de contraseña por email | Implementado | Todos |

**Reglas de negocio:** DNI, Legajo y Email son únicos. La baja es lógica (soft delete).

---

## Módulo 2 — Calificaciones y Auditoría (RF-15, CU-06)

| Caso de uso | Estado | Roles habilitados |
|---|---|---|
| Cargar nota de examen | Implementado | Docente |
| Rectificar nota de examen | Implementado | Docente, Administrador |
| Consultar historial de notas | Implementado | Estudiante, Docente, Administrador |
| Registro de auditoría de cambios | Implementado | Sistema (automático) |

**Reglas de negocio:** Nota válida entre 1 y 10. Aprobado ≥ 4. Toda carga y rectificación genera registro inmutable en `AuditoriaCambio`.

---

## Módulo 3 — Exámenes e Inscripciones (CU-22, CU-33)

| Caso de uso | Estado | Roles habilitados |
|---|---|---|
| Inscribirse a materia (autogestión) | Implementado | Estudiante |
| Inscribirse a examen final | Implementado | Estudiante |
| Inscripción a materia (manual) | Implementado | Preceptor, Administrador |
| Validación de correlatividades | Implementado | Sistema (automático) |
| Dar de baja inscripción | Implementado | Estudiante, Preceptor |
| Comprobante de inscripción | Implementado | Estudiante |

**Reglas de negocio:** Para cursar se requiere tener la materia correlativa regularizada. Para rendir examen final se requiere tener la materia aprobada.

---

## Módulo 4 — Reportes (CU-38, CU-45)

| Caso de uso | Estado | Roles habilitados |
|---|---|---|
| Tablero ejecutivo institucional (RR-01) | Implementado | Administrador |
| Reporte de inasistencias (RR-08) | Implementado | Docente, Preceptor, Administrador |
| Control de legajo individual (RR-09) | Implementado | Preceptor, Administrador |
| Comparativo entre comisiones (RR-05) | Implementado | Docente, Administrador |
| Evolución de notas por período (RR-06) | Implementado | Docente, Administrador |
| Promedios por cátedra (RR-07) | Implementado | Docente, Administrador |
| Riesgo de deserción académica | Implementado | Administrador |
| Retención y deserción por cohorte | Implementado | Administrador |

---

## Módulo 5 — Estado Académico (CU-43)

| Caso de uso | Estado |
|---|---|
| Actualizar condición del estudiante | Implementado |

**Estados posibles:** Regular, Libre, Promocional, Deserción, Egreso.

---

## Módulo 6 — Alertas Académicas (RF-10, Módulo 7)

| Caso de uso | Estado | Roles habilitados |
|---|---|---|
| Detección de riesgo por inasistencias (>25%) | Implementado | Sistema (automático, lunes 08:00) |
| Detección de riesgo por inactividad (>30 días) | Implementado | Sistema (automático, lunes 08:00) |
| Notificación de vencimientos del calendario | Implementado | Sistema (automático, lunes 08:00) |
| Disparar alertas manualmente | Implementado | Preceptor, Administrador |
| Historial de alertas generadas | Implementado | Preceptor, Administrador |

**Reglas de negocio:**
- Destinatarios: estudiante en riesgo + todos los preceptores activos + dirección.
- Deduplicación: no se reenvía la misma alerta al mismo destinatario el mismo día.
- Si el envío de email falla, la alerta igual se guarda en la BD con `Enviada = false`.
- Las alertas de vencimiento se disparan con 3 días de anticipación.

---

## Módulos Pendientes

| Módulo | Estado |
|---|---|
| Encuestas académicas anónimas (CU-36, CU-40) | Pendiente |
| Certificados con Hash SHA-256 / QR (CU-38) | Pendiente |
