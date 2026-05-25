# Sistema Académico Integral
**Instituto Superior del Profesorado en Ciencias Económicas y Jurídicas "Dr. José A. Ortiz y Herrera"**

![Angular](https://img.shields.io/badge/Angular-19-DD0031?logo=angular&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet&logoColor=white)
![Azure SQL](https://img.shields.io/badge/Azure_SQL-Database-0078D4?logo=microsoftazure&logoColor=white)
![Azure](https://img.shields.io/badge/Deploy-Azure-0078D4?logo=microsoftazure&logoColor=white)

---

## ¿Qué es este sistema?

Plataforma web de gestión académica desarrollada para centralizar y automatizar los procesos administrativos del instituto. Reemplaza planillas dispersas y correos manuales con un sistema integral que cubre:

- **Gestión de usuarios** con roles diferenciados: Administrador, Docente, Preceptor y Estudiante
- **Inscripciones** a materias y exámenes con validación automática de correlatividades
- **Calificaciones** con historial, rectificación y registro de auditoría inmutable
- **Reportes** para dirección: inasistencias, evolución de notas, comparativo por comisión y cátedra
- **Calendario académico** y seguimiento de estado académico por estudiante

---

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js + Angular CLI](https://angular.io/cli)
- Acceso a la base de datos Azure SQL (credenciales por fuera del repositorio)

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
cd backend/src && dotnet run
```

En el primer arranque, EF Core crea todas las tablas automáticamente en Azure y genera el usuario administrador inicial.

### 4. Levantar el frontend

```bash
cd frontend && ng serve
```

> **Windows:** Si PowerShell bloquea el comando `ng` con un error de "ejecución de scripts deshabilitada", ejecutar una sola vez:
> ```powershell
> Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
> ```

---

## Puertos

| Servicio | URL |
|---|---|
| Frontend | http://localhost:4200 |
| Backend API | http://localhost:5000 |

## Base de datos

La base de datos está en **Azure SQL**. Para conectarse desde un cliente visual (DBeaver, VS Code mssql):

| Campo | Valor |
|---|---|
| Server | `escmb-data-server.database.windows.net,1433` |
| Database | `escmb-db` |
| User | `admin-db` |
| Password |
| Encrypt | `True` |

---

## Comandos frecuentes

```bash
# Backend
cd backend/src && dotnet run

# Frontend
cd frontend && ng serve

# Ejecutar tests unitarios del backend
cd backend && dotnet test

# Crear una nueva migración (después de cambiar el modelo)
cd backend/src && dotnet ef migrations add NombreDeLaMigracion

# Aplicar migraciones manualmente
cd backend/src && dotnet ef database update

# Reconstruir y levantar los contenedores Docker (después de cualquier cambio en el código)
docker-compose up --build -d
```

---

## Deploy a Azure (primera vez)

### Requisitos previos

Instalar Azure CLI:
```bash
winget install Microsoft.AzureCLI
```

Instalar en VS Code las extensiones:
- **Azure App Service** (Microsoft)
- **Azure Storage** (Microsoft)

Iniciar sesión en ambas extensiones con la cuenta de Azure.

### Deploy del backend

1. Generar el build de publicación:
```bash
cd backend/src && dotnet publish -c Release -o ../publish
```

2. En VS Code → panel Azure → App Services → click derecho en `escmb-practicaprof` → **Deploy to Web App** → seleccionar la carpeta `backend/publish`.

### Deploy del frontend

1. Buildear en modo producción:
```bash
cd frontend && npm run build
```

2. En VS Code → panel Azure → Storage Accounts → storage account → Blob Containers → `$web` → subir el contenido de `frontend/dist/practica-profesional/browser`.

### URLs de producción

| Servicio | URL |
|---|---|
| Frontend | https://escmbpracticaprof.z13.web.core.windows.net |
| Backend API | https://escmb-practicaprof.azurewebsites.net |

### Apagar/encender el backend para ahorrar créditos

Portal Azure → `escmb-practicaprof` → botón **Detener** / **Iniciar**.
El frontend (Storage) y la base de datos no necesitan apagarse.

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
