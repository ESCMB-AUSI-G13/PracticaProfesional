# CLAUDE.md — Practica Profesional

## Stack tecnológico

- **Backend**: C# (.NET 8), ASP.NET Core Web API
- **Frontend**: Angular 17+
- **Base de datos**: (definir — PostgreSQL recomendado)
- **Contenedores**: Docker + Docker Compose

## Estructura del repositorio

```
PracticaProfesional/
├── backend/          # ASP.NET Core Web API
│   ├── src/
│   └── Dockerfile
├── frontend/         # Angular SPA
│   ├── src/
│   └── Dockerfile
├── docker-compose.yml
└── CLAUDE.md
```

## Convenciones

### Backend (C#)
- Arquitectura: Clean Architecture (o por capas según se defina)
- Controladores en `Controllers/`, servicios en `Services/`, modelos en `Models/`
- Nombres en PascalCase para clases y métodos
- Inyección de dependencias via `Program.cs`
- Usar `async/await` para operaciones I/O

### Frontend (Angular)
- Componentes con `ng generate component`
- Servicios con `ng generate service`
- Módulos por feature cuando corresponda
- Nombres de archivos en kebab-case
- Variables y métodos en camelCase

## Comandos frecuentes

### Levantar todo con Docker
```bash
docker-compose up --build
```

### Solo backend
```bash
cd backend
dotnet run
```

### Solo frontend
```bash
cd frontend
npm install
ng serve
```

## Puertos por defecto
- Frontend: http://localhost:4200
- Backend API: http://localhost:5000
- Base de datos: puerto según motor elegido
