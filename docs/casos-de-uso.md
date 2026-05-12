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
| Reporte de inasistencias | Implementado | Docente, Administrador |
| Comparativo entre comisiones | Implementado | Administrador |
| Evolución de notas por período | Implementado | Administrador |
| Promedios por cátedra | Implementado | Administrador |
| Control de legajo individual | Implementado | Preceptor, Administrador |

---

## Módulo 5 — Estado Académico (CU-43)

| Caso de uso | Estado |
|---|---|
| Actualizar condición del estudiante | Implementado |

**Estados posibles:** Regular, Libre, Promocional, Deserción, Egreso.

---

## Módulos Pendientes

| Módulo | Estado |
|---|---|
| Encuestas académicas anónimas (CU-36, CU-40) | Pendiente |
| Alertas automáticas de plazos (RF-10, Módulo 7) | Pendiente |
| Certificados con Hash SHA-256 / QR (CU-38) | Pendiente |
