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
cd backend && dotnet run

# Frontend
cd frontend && ng serve

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

### Build

```bash
docker-compose up --build -d
```

### Deploy del backend

En VS Code → panel Azure → App Services → click derecho en `escmb-practicaprof` → **Deploy to Web App** → seleccionar la carpeta `backend/publish`.

### Deploy del frontend

En VS Code → panel Azure → Storage Accounts → storage account → Blob Containers → `$web` → subir el contenido de `frontend/dist/practica-profesional/browser`.

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
