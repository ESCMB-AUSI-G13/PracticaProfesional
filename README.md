# Sistema Académico Integral
**Instituto Superior del Profesorado en Ciencias Económicas y Jurídicas "Dr. José A. Ortiz y Herrera"**

---

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js + Angular CLI](https://angular.io/cli)
- Acceso a la base de datos Azure SQL (pedirle la password a Pablo)

---

## Configuración inicial (primera vez)

### 1. Clonar el repositorio

```bash
git clone <url-del-repo>
cd PracticaProfesional
```

### 2. Configurar los secretos del backend

Copiar el archivo de ejemplo y completarlo con la password real:

```bash
cp backend/src/appsettings.Development.example.json backend/src/appsettings.Development.json
```

Abrir `backend/src/appsettings.Development.json` y reemplazar `INGRESA_LA_PASSWORD_AQUI` con la password de Azure SQL.

### 3. Levantar el backend

```bash
cd backend && dotnet run
```

En el primer arranque, EF Core crea todas las tablas automáticamente en Azure y genera el usuario administrador inicial.

### 4. Levantar el frontend

```bash
cd frontend && ng serve
```

---

## Puertos

| Servicio | URL |
|---|---|
| Frontend | http://localhost:4200 |
| Backend API | http://localhost:5201 |

---

## Usuario administrador (seed inicial)

| Campo | Valor |
|---|---|
| Email | `admin@institucion.edu.ar` |
| Contraseña | `Admin1234!` |
| Rol | Dirección |

---

## Base de datos

La base de datos está en **Azure SQL**. Para conectarse desde un cliente visual (DBeaver, VS Code mssql):

| Campo | Valor |
|---|---|
| Server | `escmb-data-server.database.windows.net,1433` |
| Database | `escmb-db` |
| User | `admin-db` |
| Password | *(pedirle a Pablo)* |
| Encrypt | `True` |

---

## Comandos frecuentes

```bash
# Backend
cd backend && dotnet run

# Frontend
cd frontend && ng serve

# Crear una nueva migración (después de cambiar el modelo)
cd backend/src && dotnet ef migrations add NombreDeLaMigracion

# Aplicar migraciones manualmente
cd backend/src && dotnet ef database update
```

---

## Estructura del proyecto

```
PracticaProfesional/
├── backend/
│   └── src/
│       ├── Template-API/       # Controllers (HTTP)
│       ├── Application/        # Casos de uso, DTOs, interfaces
│       ├── Domain/             # Entidades, Value Objects
│       └── Infrastructure/     # EF Core, SQL Server, servicios externos
└── frontend/                   # Angular
```
