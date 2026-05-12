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
<!-- Completar: motivación del equipo para esta elección -->

### Por qué DDD
<!-- Completar: correlatividades, historial académico y estado del estudiante como dominios ricos -->

### Auditoría inmutable
Todo cambio en Notas o Inscripciones genera un registro en `AuditoriaCambio` que no puede ser modificado ni eliminado. Esto garantiza trazabilidad completa para la dirección del instituto.

### RBAC (Control de Acceso por Rol)
Los roles son: `Administrador`, `Docente`, `Preceptor`, `Estudiante`. Cada endpoint valida el rol mediante claims del JWT. Los guards de Angular reflejan la misma lógica en el frontend.

## Diagrama de entidades principales

<!-- Agregar diagrama ER o referencia a imagen en esta carpeta -->
