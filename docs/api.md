# Referencia de API

## Documentación interactiva (Swagger)

El backend expone documentación automática via Swagger UI:

- **Local:** http://localhost:5000/swagger
- **Producción:** no disponible (solo activo en entorno Development)

---

## Autenticación

Todos los endpoints (excepto `/api/auth/login` y `/api/auth/forgot-password`) requieren un header:

```
Authorization: Bearer <token>
```

El token se obtiene del endpoint de login y tiene una duración configurada en `appsettings.json`.

---

## Endpoints principales

### Auth
| Método | Endpoint | Descripción |
|---|---|---|
| POST | `/api/auth/login` | Login, retorna JWT |
| POST | `/api/auth/forgot-password` | Solicitar reset de contraseña |
| POST | `/api/auth/reset-password` | Confirmar nueva contraseña |

### Usuarios
| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/usuarios` | Listar usuarios |
| POST | `/api/usuarios` | Crear usuario |
| PUT | `/api/usuarios/{id}` | Modificar usuario |
| DELETE | `/api/usuarios/{id}` | Desactivar usuario |

### Docentes / Preceptores / Estudiantes
Siguen el mismo patrón que Usuarios. Ver Swagger para detalle completo.

### Inscripciones
| Método | Endpoint | Descripción |
|---|---|---|
| POST | `/api/inscripciones/materia` | Inscribirse a materia |
| POST | `/api/inscripciones/examen` | Inscribirse a examen |
| DELETE | `/api/inscripciones/materia/{id}` | Dar de baja inscripción |
| GET | `/api/inscripciones/mis-inscripciones` | Listar inscripciones del estudiante |

### Calificaciones
| Método | Endpoint | Descripción |
|---|---|---|
| POST | `/api/calificaciones` | Cargar nota |
| PUT | `/api/calificaciones/{id}` | Rectificar nota |
| GET | `/api/calificaciones/historial/{estudianteId}` | Historial de notas |

### Reportes
| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/reportes/inasistencias` | Reporte de inasistencias |
| GET | `/api/reportes/comisiones` | Comparativo entre comisiones |
| GET | `/api/reportes/evolucion` | Evolución de notas |
| GET | `/api/reportes/catedras` | Promedios por cátedra |
| GET | `/api/reportes/legajo/{legajo}` | Control individual por legajo |

### Alertas
| Método | Endpoint | Roles | Descripción |
|---|---|---|---|
| POST | `/api/alertas/detectar-riesgo` | Preceptor, Dirección | Ejecuta la detección de riesgo académico manualmente |
| POST | `/api/alertas/vencimientos` | Preceptor, Dirección | Notifica vencimientos del Calendario Académico en los próximos 3 días |
| GET | `/api/alertas` | Preceptor, Dirección | Lista alertas generadas; filtros opcionales: `?tipo=RiesgoAsistencia&enviada=true` |
| POST | `/api/alertas/test-email` | — (solo DEV) | Envía un email de prueba a la dirección indicada. Retorna 404 en producción |

**Respuesta de POST detectar-riesgo / vencimientos:**
```json
{
  "alertasGeneradas": 3,
  "emailsEnviados": 3,
  "detalles": [
    "Alerta RiesgoAsistencia para Juan Pérez (est. 12) — email a juan@mail.com OK",
    "Alerta VencimientoCargaNotas para Matemáticas I — email a docente@mail.com OK"
  ]
}
```

**Valores de `tipo` (enum `TipoAlerta`):**
| Valor | Descripción |
|---|---|
| `RiesgoAsistencia` | Estudiante supera el 25% de ausencias injustificadas |
| `RiesgoInactividad` | Estudiante sin actividad hace más de 30 días |
| `VencimientoCargaNotas` | Fecha límite de carga de notas próxima a vencer |
| `VencimientoInscripcion` | Período de inscripción próximo a cerrar |

---

### Notificaciones
| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/notificaciones` | Lista las notificaciones del usuario autenticado (máx. 50, ordenadas por fecha desc.) |
| PATCH | `/api/notificaciones/{id}/leida` | Marca una notificación como leída (debe pertenecer al usuario autenticado) |
| PATCH | `/api/notificaciones/marcar-todas-leidas` | Marca todas las notificaciones del usuario como leídas |

**Respuesta de GET /api/notificaciones:**
```json
[
  {
    "id": 1,
    "titulo": "Alerta de riesgo académico",
    "mensaje": "El estudiante Juan Pérez supera el 25% de ausencias en Matemáticas I.",
    "leida": false,
    "fechaCreacion": "2026-05-25T08:00:00",
    "tipo": "RiesgoAsistencia"
  }
]
```
