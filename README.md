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
| SQL Server | localhost:1433 |

## Usuarios
Email: admin@institucion.edu.ar  
Contraseña: Admin1234!  
Rol: Direccion

## Conexión visual a la base de datos

Con los contenedores corriendo, conectarse desde cualquier cliente SQL:

| Campo | Valor |
|---|---|
| Server | `localhost,1433` |
| User | `sa` |
| Password | `YourStrong!Passw0rd` |
| Database | `PracticaProfesionalDB` |

Clientes:
- **DBeaver**
- **Extensión SQL Server (mssql)** en VS Code

## Consultar la base de datos desde consola

Con los contenedores corriendo (`docker-compose up`), conectarse al SQL Server:

```bash
# Abrir una terminal dentro del contenedor de SQL Server
docker exec -it practicaprofesional-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong!Passw0rd" -No -C
```

Una vez dentro del prompt `1>`, ejecutar consultas:

```sql
-- Seleccionar la base de datos
USE PracticaProfesionalDB;
GO

-- Ver todos los usuarios
SELECT Id, DNI, Legajo, Email, Nombre, Apellido, Rol, Activo, FechaCreacion
FROM Usuarios;
GO

-- Ver solo usuarios activos
SELECT Id, Legajo, Email, Nombre, Apellido, Rol
FROM Usuarios
WHERE Activo = 1;
GO

-- Salir
EXIT
```

O en un solo comando sin entrar al prompt interactivo:

```bash
docker exec -it practicaprofesional-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong!Passw0rd" -No -C \
  -Q "USE PracticaProfesionalDB; SELECT Id, Legajo, Email, Nombre, Apellido, Rol, Activo FROM Usuarios;"
```
