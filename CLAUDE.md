# CLAUDE.md — Práctica Profesional: Sistema Académico Integral

## Contexto del Proyecto
**Institución:** Instituto Superior del Profesorado en Ciencias Económicas y Jurídicas “Dr. José A. Ortiz y Herrera”.
**Problema:** Procesos fragmentados en planillas y correos, falta de alertas para docentes, inscripciones manuales y dificultad en el seguimiento de trayectorias (egresados/desertores).
**Objetivo:** Centralizar y automatizar la gestión académica, integrando encuestas, historial, exámenes y reportes para mejorar la toma de decisiones.

---

## Arquitectura (OBLIGATORIO)

Este proyecto usa **Clean Architecture simplificada con DDD**.
Toda implementación DEBE respetar la separación de capas. NO se permite mezclar responsabilidades.

### Estructura del Backend

```
backend/
└── src/
├── Template-API/    # Entry point (HTTP Controllers)
├── Application/     # Casos de uso (Commands/Queries), DTOs, Interfaces
├── Domain/          # Lógica de negocio pura (Entities, VOs, Domain Events)
└── Infrastructure/  # EF Core, SQL Server, Mail Service, Auth
```

### Flujo de ejecución
`Controller → UseCase → Domain → Repository → Infrastructure`

---

## Reglas por Capa

### Template-API (Controllers)
- Maneja requests HTTP y llama a UseCases. **NO** lógica de negocio.

### Application
- Contiene **UseCases**, DTOs e interfaces de Repositorios.
- **NO** usar "Services" genéricos. Cada feature es un UseCase.

### Domain
- Núcleo inmutable: Entities, Value Objects y Domain Events.
- **NO** dependencias externas.

### Infrastructure
- Implementación de interfaces, acceso a **SQL Server (EF Core)** y servicios externos (Notificaciones).

---
## Módulos y Requerimientos Críticos

### 1. Gestión de Usuarios y Roles (CU-02)
- Alta/Baja/Modificación de Docentes, Preceptores y Estudiantes.
- **RBAC:** Permisos diferenciados por rol.
- **Validación:** DNI, Legajo y Correo Institucional únicos.

### 2. Calificaciones y Alertas (RF-15, RF-10)
- Carga de notas con validación de integridad referencial.
- **Alertas (Módulo 7):** Envío automático de recordatorios sobre plazos de carga de notas.
- **Temporalidad (CU-47):** Validar contra Calendario Académico.

### 3. Exámenes e Inscripciones (CU-22, CU-33)
- Inscripción autogestionada con **validación automática de correlatividades**.
- **Regla:** Habilitación para cursar (Regularizado) vs. Habilitación para rendir (Aprobado).
- Control de períodos de inscripción y cierre de actas para Preceptores.

### 4. Encuestas Académicas (CU-36, CU-40)
- **Obligatoriedad:** Aplicación automática al momento de inscribirse a materias/exámenes.
- **Anonimización:** Algoritmo de disociación de identidad (Sin FK al Alumno).

### 5. Historial y Máquina de Estados (CU-43)
- Tracking de condición: Regular, Libre, Promocional, Egreso, Deserción.
- **Evento MateriaAprobada:** Si el plan está 100% completo, transición automática a `Egreso`.

### 6. Reportes e Indicadores (CU-38, CU-45)
- Dashboards para Dirección: Tasas de aprobación, deserción y rendimiento por cohorte.
- Exportación de certificados con **Hash SHA-256 / QR** de integridad.
---

## Reglas Estrictas (GLOBAL)

- **DB:** El motor de base de datos es **SQL Server**.
- **NO** crear carpetas `Services` o `Utils` genéricas.
- **Auditoría (CU-06):** Todo cambio en Notas o Inscripciones debe generar un registro **inmutable** en `Auditoria_Cambio`.
- **Invariante de Nota:** Value Object con rango 1-10. Aprobado ≥ 4.

---

## Convenciones Técnicas

### Backend (C# / .NET)
- **Async/Await:** Obligatorio en I/O y Repositorios.
- **Error Handling:** Middleware global para `BusinessException` (400/409).
- **DI:** Registro en `Program.cs` o módulos de capa.

### Frontend (Angular)
- **Estructura:** Carpetas por feature (ej. `features/inscripciones`).
- **State:** Componentes ligeros; lógica delegada a servicios de Angular.
- **Naming:** Archivos en `kebab-case`.

---

## Modelo de Dominio (Resumen)

| Entidad | Atributos Clave | Lógica Crítica |
|---|---|---|
| **Usuarios** | DNI, Legajo, Email, Rol | Validar unicidad, Autenticar |
| **Materias** | Código, Nombre, Plan | Motor de Correlatividades |
| **Inscripción** | ID_Alumno, ID_Materia, Estado | Validar requisitos previos |
| **Historial** | Nota, Condición, Año | Cálculo de promedios, Alerta Deserción |
| **Auditoría** | Acción, ValorAnterior, ValorNuevo | Registro inmutable de cambios |

---

## Comandos y Puertos

- **Backend:** `cd backend && dotnet run` (Puerto 5000)
- **Frontend:** `cd frontend && ng serve` (Puerto 4200)
- **Base de Datos:** SQL Server

## Protocolo de Git
1. `git status`
2. `git checkout -b feature/[nombre-breve]`
3. `git add .`
4. `git commit -m "feat: [descripción en español]"`
5. `git push origin [rama]`