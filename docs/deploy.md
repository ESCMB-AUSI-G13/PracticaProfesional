# Deploy a Azure

La aplicación está desplegada en tres recursos de Azure independientes:

| Recurso | Servicio Azure | URL |
|---|---|---|
| Frontend | Azure Blob Storage (Static Website) | https://escmbpracticaprof.z13.web.core.windows.net |
| Backend | Azure App Service | https://escmb-practicaprof.azurewebsites.net |
| Base de datos | Azure SQL Database | `escmb-data-server.database.windows.net` |

---

## CI/CD con GitHub Actions

El deploy es **automático** desde GitHub Actions. Hay dos workflows en `.github/workflows/`:

| Workflow | Se dispara cuando... | Qué hace |
|---|---|---|
| `deploy-backend.yml` | push a `main` que toca `backend/**` | corre los tests → `dotnet publish` → deploy a App Service |
| `deploy-frontend.yml` | push a `main` que toca `frontend/**` | `npm ci` → `ng build --configuration production` → sube `dist` al contenedor `$web` |

Si algún **test falla**, el deploy del backend se cancela y lo último que quedó en producción sigue sirviendo.

También se pueden correr a mano: GitHub → pestaña **Actions** → elegir el workflow → **Run workflow**.

### Secrets requeridos (GitHub → Settings → Secrets and variables → Actions)

| Secret | Valor |
|---|---|
| `AZURE_WEBAPP_PUBLISH_PROFILE` | Contenido del publish profile del App Service (Overview → Download publish profile) |
| `AZURE_STORAGE_ACCOUNT` | Nombre de la Storage Account |
| `AZURE_STORAGE_KEY` | Access key (key1) de la Storage Account |

> Nota: el publish profile requiere que "SCM Basic Auth Publishing Credentials" esté en **On** en Configuration → General settings del App Service.

### Deploy manual (alternativa)

Si hiciera falta deployar sin pasar por GitHub (ej. sin conexión a internet del runner):

**Backend** — VS Code → panel Azure → App Services → click derecho en `escmb-practicaprof` → **Deploy to Web App** → seleccionar `backend/publish`.

**Frontend**:
```bash
cd frontend && ng build --configuration production
```
En VS Code → Storage Accounts → `$web` → subir el contenido de `frontend/dist/practica-profesional/browser`.

---

## Gestión de costos

El frontend (Blob Storage) y la base de datos pueden quedar encendidos sin costo significativo.

El **backend (App Service)** se puede apagar cuando no se usa:
- Portal Azure → `escmb-practicaprof` → botón **Detener** / **Iniciar**

---

## Variables de entorno del backend

Las credenciales de producción están configuradas en las **Application Settings** del App Service en el portal de Azure, no en el código fuente. El separador de jerarquía es `__` (doble guión bajo), ej. `GeminiIA__ApiKey` mapea a `GeminiIA:ApiKey`.

**Asistente de IA:** requiere `GeminiIA__ApiKey` — una API key **gratuita** de [Google AI Studio](https://aistudio.google.com) (no pide tarjeta). Sin esta variable configurada, el botón flotante del asistente sigue visible para Dirección pero devuelve un error de "no disponible" en vez de romper el resto de la app.
