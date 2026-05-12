# Referencia de API

## Documentación interactiva (Swagger)

El backend expone documentación automática via Swagger UI:

- **Local:** http://localhost:5201/swagger
- **Producción:** https://escmb-practicaprof.azurewebsites.net/swagger

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
