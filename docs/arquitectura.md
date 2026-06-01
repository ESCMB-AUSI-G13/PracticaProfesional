# Arquitectura del Sistema

## Visión general

El sistema sigue **Clean Architecture simplificada con DDD (Domain-Driven Design)**. La separación estricta de capas garantiza que la lógica de negocio no dependa de frameworks ni de la base de datos.

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Frontend | Angular 19 |
| Backend | .NET 8 (ASP.NET Core) |
| Base de datos | Azure SQL (SQL Server) |
| Autenticación | JWT (JSON Web Tokens) |
| Deploy | Azure App Service (backend) + Azure Static Web / Blob Storage (frontend) |

## Estructura del backend

```
backend/src/
├── Controllers/        # Entry point HTTP — solo recibe requests y llama UseCases
├── Application/        # Casos de uso, DTOs, interfaces de repositorios
├── Domain/             # Entidades, Value Objects, Enums — sin dependencias externas
└── Infrastructure/     # EF Core, SQL Server, JWT, Email, Auditoría
```

### Flujo de ejecución

```
HTTP Request → Controller → UseCase → Domain → Repository (Interface)
                                                      ↓
                                              Infrastructure (EF Core)
```

## Decisiones de diseño

### Por qué Clean Architecture

En una arquitectura en capas clásica (MVC), la lógica de negocio termina acoplada al framework o a la base de datos. En este sistema eso sería un problema: las reglas de correlatividades, la máquina de estados del historial académico y las validaciones del calendario son complejas y cambian de forma independiente a la tecnología.

Clean Architecture resuelve esto invirtiendo las dependencias: la capa de dominio no importa nada de EF Core, ASP.NET ni SQL Server. Esto permite:

- **Testear la lógica de negocio de forma aislada**, sin base de datos ni servidor (ver suite en `backend/tests/`).
- **Cambiar la infraestructura** (por ejemplo, migrar de SQL Server a PostgreSQL) sin tocar el dominio ni los casos de uso.
- **Mantener los casos de uso cohesivos**: cada feature es un `UseCase` con una sola responsabilidad, en lugar de servicios genéricos que crecen sin control.

### Por qué DDD

El dominio del instituto tiene reglas de negocio ricas que no son simples CRUDs:

- **Correlatividades**: si un estudiante puede inscribirse a una materia depende de si tiene otras materias regularizadas o aprobadas. Esta lógica vive en el dominio y no se delega a la base de datos.
- **Máquina de estados del historial**: un estudiante transita por estados (`Regular`, `Libre`, `Promocional`, `Egresado`, `Desertor`) con transiciones explícitas y validadas. Modelarlo como un enum con lógica de transición en el `Estudiante` evita estados inválidos.
- **Value Objects con invariantes**: `Nota` no es un `decimal` cualquiera — tiene rango 1–10 y sabe si es aprobada (≥ 4). Encapsular eso en un VO garantiza que nunca haya una nota de 0 o de 15 en el sistema.

DDD permite que el código hable el mismo lenguaje que el instituto (legajo, correlatividad, regularidad, espacio curricular) en lugar de traducir constantemente entre términos técnicos y del negocio.

### Auditoría inmutable
Todo cambio en Notas o Inscripciones genera un registro en `AuditoriaCambio` que no puede ser modificado ni eliminado. Esto garantiza trazabilidad completa para la dirección del instituto.

### RBAC (Control de Acceso por Rol)
Los roles son: `Administrador`, `Docente`, `Preceptor`, `Estudiante`. Cada endpoint valida el rol mediante claims del JWT. Los guards de Angular reflejan la misma lógica en el frontend.

## Tests unitarios

El proyecto incluye una suite de tests unitarios del dominio ubicada en `backend/tests/`. No requieren base de datos ni levantar el servidor — testean exclusivamente la lógica de negocio pura.

```bash
cd backend && dotnet test
```

### Qué se testea

| Clase | Casos cubiertos |
|---|---|
| `Nota` | Rango 1-10, límite de aprobación (≥ 4), redondeo, equality |
| `Estudiante` | Todas las transiciones de estado, estado terminal Egresado, idempotencia, domain events |
| `InscripcionExamen` | CargarNota, RectificarNota (solo desde Aprobada/Desaprobada), DarDeBaja |
| `Examen` | Validaciones de fecha futura, horario no vacío, cupo positivo |
| `Correlatividad` | No auto-referencia, tipo de requerimiento obligatorio, condición Regularizado vs Aprobado |
| `InscripcionMateria` | Ciclo de vida: Activa → Baja / Aprobada / Desaprobada |
| `PadronAlumno` | Validación de formato DNI (solo dígitos, 7-10 caracteres), trim de espacios, fecha de carga |

### Por qué solo dominio

Los UseCases y Controllers dependen de repositorios e infraestructura (EF Core, SQL Server). Testearlos requiere mocks o una base de datos real, lo que añade complejidad sin agregar valor para este proyecto. La lógica crítica — reglas de negocio, invariantes, máquina de estados — vive en el dominio y es exactamente lo que se testea aquí.

## Entidades principales y relaciones

| Entidad | Relaciones clave |
|---|---|
| `Usuario` | Base de Docente, Preceptor, Estudiante |
| `Estudiante` | tiene muchas `InscripcionMateria`, `InscripcionExamen`, `Asistencia`, `Notificacion` |
| `Materia` | pertenece a una Carrera; tiene `Correlatividad` con otras materias |
| `Curso` | agrupa estudiantes por año lectivo y comisión; tiene un Preceptor asignado |
| `EspacioCurricular` | une Docente + Materia + Curso; es el contexto de asistencias y notas |
| `InscripcionMateria` | Estudiante ↔ EspacioCurricular; estados: Activa, Aprobada, Desaprobada, Baja |
| `Examen` | pertenece a una Materia; tiene inscriptos (`InscripcionExamen`) |
| `InscripcionExamen` | Estudiante ↔ Examen; guarda la nota y condición final |
| `Asistencia` | registro diario por Estudiante + EspacioCurricular |
| `CalendarioAcademico` | eventos del año lectivo con fechas de inicio/fin y tipo |
| `Alerta` | generada automáticamente; referencia opcional a Estudiante o CalendarioAcademico |
| `Notificacion` | in-app; pertenece a un Usuario; se genera junto a cada Alerta |
| `AuditoriaCambio` | registro inmutable de cambios en notas e inscripciones |
