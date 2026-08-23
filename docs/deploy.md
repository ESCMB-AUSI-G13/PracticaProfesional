# Deploy a Azure

La aplicación está desplegada en tres recursos de Azure independientes:

| Recurso | Servicio Azure | URL |
|---|---|---|
| Frontend | Azure Blob Storage (Static Website) | https://escmbpracticaprof.z13.web.core.windows.net |
| Backend | Azure App Service | https://escmb-practicaprof.azurewebsites.net |
| Base de datos | Azure SQL Database | `escmb-data-server.database.windows.net` |

---

## Requisitos previos

- Azure CLI instalado: `winget install Microsoft.AzureCLI`
- Extensiones de VS Code: **Azure App Service** y **Azure Storage**
- Sesión iniciada en ambas extensiones con la cuenta del instituto

---

## Deploy del backend

En VS Code → panel Azure → App Services → click derecho en `escmb-practicaprof` → **Deploy to Web App** → seleccionar `backend/publish`.

---

## Deploy del frontend

1. Buildear:
```bash
cd frontend && ng build --configuration production
```

2. En VS Code → panel Azure → Storage Accounts → `$web` → subir el contenido de `frontend/dist/practica-profesional/browser`.

---

## Gestión de costos

El frontend (Blob Storage) y la base de datos pueden quedar encendidos sin costo significativo.

El **backend (App Service)** se puede apagar cuando no se usa:
- Portal Azure → `escmb-practicaprof` → botón **Detener** / **Iniciar**

---

## Variables de entorno del backend

Las credenciales de producción están configuradas en las **Application Settings** del App Service en el portal de Azure, no en el código fuente. El separador de jerarquía es `__` (doble guión bajo), ej. `GeminiIA__ApiKey` mapea a `GeminiIA:ApiKey`.

**Asistente de IA:** requiere `GeminiIA__ApiKey` — una API key **gratuita** de [Google AI Studio](https://aistudio.google.com) (no pide tarjeta). Sin esta variable configurada, el botón flotante del asistente sigue visible para Dirección pero devuelve un error de "no disponible" en vez de romper el resto de la app.
