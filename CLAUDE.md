# CLAUDE.md — Practica Profesional

## Arquitectura (OBLIGATORIO)

Este proyecto usa **Clean Architecture simplificada con DDD**.
Toda implementación DEBE respetar la separación de capas. NO se permite mezclar responsabilidades.

### Estructura del Backend

```
backend/
└── src/
    ├── Template-API/    # Entry point (HTTP)
    ├── Application/     # Casos de uso
    ├── Domain/          # Lógica de negocio pura
    └── Infrastructure/  # Implementaciones externas
```

### Flujo de ejecución

```
Controller → UseCase → Domain → Repository → Infrastructure
```

---

## Reglas por capa

### Template-API (Controllers)
- Maneja requests HTTP y llama a UseCases.
- **NO** contiene lógica de negocio ni accede a base de datos.

### Application
- Contiene UseCases (Commands / Queries), DTOs e interfaces de Repositorios.
- **NO** usar "Services" genéricos ni acceder a Infrastructure directamente.

### Domain
- Núcleo: Entities, Value Objects y Domain Events.
- **NO** dependencias externas ni lógica de infraestructura.

### Infrastructure
- Implementa interfaces de Application, acceso a BD (EF Core) y servicios externos.

---

## Dominio y Funcionalidades

Al generar código, basarse en estos Requerimientos y Casos de Uso:

- **Gestión Académica**: Motor de Correlatividades (CU-22), Carga de Notas (RF-15) y Ciclo de Vida del Alumno (CU-43).
- **Seguridad**: Auditoría inmutable de cambios (CU-06) y Roles de Usuario (CU-02).
- **Documentación**: Generación de PDFs con Hash/QR de integridad (CU-33).
- **Encuestas**: Algoritmo de disociación de identidad (CU-36/40).
- **Reportes**: Indicadores de deserción y rendimiento académico (CU-38/45).

---

## Reglas estrictas (GLOBAL)

- **NO** crear carpeta `Services` genérica ni `Utils`.
- **NO** mezclar capas ni duplicar lógica.
- Cada feature debe implementarse como un **UseCase**.
- Si una implementación rompe la arquitectura, mezcla capas o ignora las reglas de negocio del Instituto, **DEBE ser rechazada**.

---

## Convenciones técnicas

### Backend (C# / .NET)

- **Naming**: Clases y Métodos en `PascalCase`, variables en `camelCase`.
- **Async/Await**: Uso obligatorio en todas las llamadas de I/O y Repositorios.
- **Dependency Injection**: Todo se registra en `Program.cs` o mediante módulos de capa.
- **Error Handling**: Middleware global para capturar `BusinessException` y devolver `400`/`409` según corresponda.

### Frontend (Angular)

- **Estructura**: Carpetas por feature (ej. `features/inscripciones`, `features/reportes`).
- **Naming**: Archivos y carpetas en `kebab-case`.
- **State**: Los componentes no deben manejar lógica compleja; delegar a servicios de Angular.
- Uso de servicios para llamadas al backend.

---

## Reglas de Negocio y Dominio

### 1. Motor de Correlatividades (CU-22)

El dominio debe implementar validación doble sobre el historial del alumno:

- **Habilitación para Cursar**: Correlativas previas en estado `Regularizado`.
- **Habilitación para Rendir**: Correlativas previas en estado `Aprobado` (Nota ≥ 4).
- **Bloqueo**: Si no se cumple la condición, el UseCase lanza una `BusinessException`.

### 2. Gestión de Calificaciones (RF-15, RF-10)

- **Value Object Nota**: Rango válido de 1 a 10 (inclusivo).
- **Aprobación**: Nota ≥ 4 → Aprobado. Nota < 4 → Libre / Reprobado.
- **Restricción de Carga**: Solo usuarios con Rol `Docente` asignados a la materia o `Secretaría` pueden modificar notas.
- **Temporalidad (CU-47)**: Validar contra el Calendario Académico que la carga esté dentro del rango de fechas permitido.

### 3. Máquina de Estados del Alumno (CU-43)

- **Evento `MateriaAprobada`**: Al registrar nota ≥ 4, verificar si es la última materia del Plan de Estudios.
- **Transición a Egreso**: Si el plan está 100% completo, el estado cambia automáticamente a `Egreso`.
- **Pérdida de Regularidad**: Calcular automáticamente condición `Libre` por inasistencias o falta de aprobación de finales.

### 4. Seguridad, Auditoría e Inmutabilidad (CU-02, CU-06)

- **Auditoría Obligatoria**: Todo cambio en Notas, Usuarios o Inscripciones debe disparar un Domain Event que cree un registro en `Auditoria_Cambio`.
- **Invariante de Auditoría**: Los registros son **inmutables** — solo `Create` y `Read`, nunca `Update` ni `Delete`.
- **RBAC**: Validar en Application que `User.Role` tenga permisos para ejecutar el UseCase específico.

### 5. Privacidad y Algoritmos (CU-33, CU-36)

- **Anonimización de Encuestas**: Transacción atómica:
  1. Marcar al Alumno como "Encuesta Realizada".
  2. Guardar la Respuesta **sin** ninguna FK al `ID_Alumno`.
- **Integridad de Documentos**: Al generar certificados, calcular Hash SHA-256 del contenido y adjuntarlo como QR de validación.

### 6. Integridad del Padrón (RF-02, RF-04)

- **Value Object DNI / Legajo**: Validar formato numérico y longitud.
- **Unicidad**: No permitir dos Personas con el mismo DNI o Legajo.

---

## Modelo de Dominio

### Gestión de Accesos (Seguridad)

| Entidad | Atributos clave | Métodos / Lógica |
|---|---|---|
| Usuarios | ID_Usuario, Username, Contraseña (hash), Estado, Email, DNI, NumeroTelefono, Direccion, ID_Rol | `Autenticar()`, `CambiarEstado()`, `ValidarFormatoEmail()`, `RestablecerContraseña()` |
| Roles | ID_Rol, Nombre (Docente, Estudiante, etc.) | `AsignarRol()`, `ConsultarNivelAcceso()` |
| Permisos | ID_Permiso, Nombre, Descripcion | `VerificarHabilitacion()` |
| Permiso_Rol | ID_RolPermiso, ID_Rol, ID_Permiso | `AsignarPermisoARol()`, `ListarPermisosPorRol()` |

### Perfil de Usuario (Relación 1:1 con Usuario)

| Entidad | Atributos clave | Métodos / Lógica |
|---|---|---|
| Estudiantes | ID_Usuario_Estudiante, Legajo_Estudiante, Fecha_Ingreso, Condicion_Actual | `ActualizarCondicionActual()`, `VincularLegajo()` |
| Docentes | ID_Usuario_Docente, Legajo_Docente, Categoría | `ActualizarCategoría()` |
| Preceptores | ID_Usuario_Preceptor, Legajo_Preceptor | `VincularComisión()` |

> **Nota**: `Usuario` es la entidad base. `Estudiantes`, `Docentes` y `Preceptores` heredan `ID_Usuario` como PK/FK, extendiendo atributos generales con datos específicos (Legajos).

### Estructura Académica

| Entidad | Atributos clave | Métodos / Lógica |
|---|---|---|
| Materias | ID_Materia, Codigo, Nombre | `ValidarCodigoUnico()`, `ConsultarPlanDeEstudio()` |
| Curso | ID_Curso, Año, Comision, Cupo, Estado, ID_Usuario_Preceptor | `ValidarCupoDisponible()`, `CerrarCurso()`, `AsignarPreceptor()` |
| Espacio_Curricular | ID_Esp_Curricular, ID_Usuario_Docente, ID_Materia, ID_Usuario_Estudiante, ID_Curso | `AsignarDocenteAMateria()`, `CrearVínculoAcademico()` |
| Correlatividad | ID_Correlatividad, Tipo_Requerimiento, Condicion_Academica, ID_Materia_Destino, ID_Materia_Objetivo | `VerificarRequisitos()` |

### Procesos Académicos

| Entidad | Atributos clave | Métodos / Lógica |
|---|---|---|
| Inscripcion_Materia | ID_Inscripcion_Mat, ID_Usuario_Estudiante, ID_Materia, ID_Curso, Estado, Fecha_Inscripcion | `ValidarCorrelatividadParaCursar()`, `CambiarEstadoInscripcion()` |
| Examen | ID_Examen, Fecha, Horario, Tipo (Final/Parcial), ID_Materia | `AbrirMesaExamen()`, `GenerarActaVolante()` |
| Inscripcion_Examen | ID_Insc_Examen, Cupo, Fecha_Examen, Horario, Estado, ID_Usuario_Estudiante, ID_Examen | `ValidarCorrelatividadParaRendir()`, `RegistrarPresentismo()` |
| Asistencia | ID_Asistencia, Fecha, Estado (Presente/Ausente), Motivo, ID_Materia, ID_Usuario_Estudiante | `RegistrarAsistencia()`, `CalcularPorcentajeAsistencia()` |

### Seguimiento e Historia

| Entidad | Atributos clave | Métodos / Lógica |
|---|---|---|
| Historial_Academico | ID_Historial, Año, Comision, Estado_Final, Nota_Final, Condicion, ID_Materia, ID_Usuario_Estudiante, ID_Curso | `ConsolidarResultadoFinal()`, `CalcularPromedio()`, `DispararEstadoEgreso()` |
| Alertas | ID_Alerta, Año, Comision, Estado_Final, Nota_Final, Condicion, IDs de Inscripción | `GenerarNotificaciónAutomática()`, `ValidarPlazosVencidos()` |
| Auditoria_Cambio | ID_Cambio, Tabla_Afectada, Registro_Afectado, Accion (INSERT/UPDATE/DELETE), Fecha_Cambio, Valor_Nuevo, ID_Usuario | `RegistrarLogInmutable()`, `ConsultarTrazabilidad()` |

### Calidad (Encuestas)

| Entidad | Atributos clave | Métodos / Lógica |
|---|---|---|
| Encuesta | ID_Encuesta, Preguntas, ID_Materia, ID_Usuario_Docente | `CrearInstrumentoEvaluacion()` |
| Respuestas_Encuesta | ID_Respuesta, Preguntas, Respuestas, Fecha, ID_Encuesta | `GuardarRespuestaAnonima()`, `EjecutarAlgoritmoDisociacion()` |

---

## Comandos frecuentes

```bash
# Levantar todo con Docker
docker-compose up --build

# Solo backend
cd backend && dotnet run

# Solo frontend
cd frontend && ng serve
```

## Puertos por defecto

| Servicio | Puerto |
|---|---|
| Frontend | http://localhost:4200 |
| Backend API | http://localhost:5000 |
| Base de datos | según motor elegido |

## Protocolo Automatizado de Git (Comandos)
Cuando el usuario pida "Subir cambios" o "Guardar progreso", ejecutar esta secuencia:
1. **Status:** `git status` para identificar archivos modificados en /backend, /frontend o /docs.
2. **Rama:** Si estamos en 'main', crear rama nueva: `git checkout -b feature/[nombre-breve]`. Si ya estamos en una feature, seguir ahí.
3. **Stage:** `git add .` (asegurar que no se suban archivos de /bin o /obj de C#).
4. **Commit:** `git commit -m "feat: [descripción clara de los cambios en español]"`
5. **Push:** `git push origin [nombre-de-la-rama-actual]`
6. **Finalizar:** Avisar al usuario: "Cambios subidos. Revisar en GitHub Desktop para el Pull Request".