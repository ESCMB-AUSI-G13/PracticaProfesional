using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

public static class EstudiantesSeeder
{
    private static readonly string[] Nombres =
    [
        "Lucas", "Valentina", "Mateo", "Sofía", "Santiago", "Martina",
        "Tomás", "Camila", "Joaquín", "Lucía", "Nicolás", "Isabella",
        "Agustín", "Emma", "Facundo", "Julieta", "Ignacio", "Florencia",
        "Franco", "Carla", "Ezequiel", "Natalia", "Maximiliano", "Paula",
        "Rodrigo", "Antonella", "Bruno", "Victoria", "Lautaro", "Milagros"
    ];

    private static readonly string[] Apellidos =
    [
        "González", "Rodríguez", "Gómez", "Fernández", "López",
        "Díaz", "Martínez", "Pérez", "García", "Torres",
        "Álvarez", "Romero", "Sánchez", "Ruiz", "Ramírez",
        "Flores", "Acosta", "Medina", "Herrera", "Castro",
        "Morales", "Ortiz", "Silva", "Vega", "Molina",
        "Ramos", "Cruz", "Suárez", "Reyes", "Gutiérrez"
    ];

    private sealed record Grupo(
        int CarreraId,
        int AnioEstudiante,
        string Comision,
        int CursoId,
        int[] MateriaIds,
        int AnioIngreso
    );

    private static readonly Grupo[] Grupos =
    [
        // ── Profesorado de Educación Secundaria en Economía (CarreraId = 1) ──
        new(1, 1, "A", 1,  [17, 18, 19, 20, 21, 22, 23, 24, 48],         2026),
        new(1, 1, "B", 2,  [17, 18, 19, 20, 21, 22, 23, 24, 48],         2026),
        new(1, 2, "A", 14, [25, 26, 27, 28, 29, 30, 31, 49, 50],         2025),
        new(1, 2, "B", 15, [25, 26, 27, 28, 29, 30, 31, 49, 50],         2025),
        new(1, 3, "A", 16, [32, 33, 34, 35, 36, 37, 38, 39, 51],         2024),
        new(1, 3, "B", 17, [32, 33, 34, 35, 36, 37, 38, 39, 51],         2024),
        new(1, 4, "A", 18, [40, 41, 42, 43, 44, 45, 46, 47, 52, 53],     2023),
        new(1, 4, "B", 19, [40, 41, 42, 43, 44, 45, 46, 47, 52, 53],     2023),

        // ── Trayecto Pedagógico para Graduados No Docentes (CarreraId = 2) ──
        new(2, 1, "A", 1,  [4, 6, 7, 8, 9, 10, 11],                      2026),
        new(2, 1, "B", 2,  [4, 6, 7, 8, 9, 10, 11],                      2026),
        new(2, 2, "A", 14, [12, 13, 14, 15, 16],                          2025),
        new(2, 2, "B", 15, [12, 13, 14, 15, 16],                          2025),
    ];

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // Omitir sólo si el seed completo ya fue ejecutado (>= 350 estudiantes)
        if (await db.Estudiantes.CountAsync(ct) >= 350)
        {
            logger.LogInformation("EstudiantesSeeder: la BD ya tiene suficientes estudiantes, seed omitido.");
            return;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Alumno2026!");
        int dniBase = 40_000_201;  // los 180 anteriores usaron 40000001–40000180
        int totalInsertados = 0;

        // Pre-carga legajos ya existentes para saltar duplicados (idempotencia)
        var legajosExistentes = (await db.Usuarios
            .Where(u => u.Legajo.StartsWith("EST-"))
            .Select(u => u.Legajo)
            .ToListAsync(ct))
            .ToHashSet();

        int globalIdx = 0;   // índice acumulado entre todos los grupos → nombres únicos

        foreach (var grupo in Grupos)
        {
            var prefijo = $"C{grupo.CarreraId}-{grupo.AnioEstudiante}{grupo.Comision}";
            var fechaIngreso = new DateTime(grupo.AnioIngreso, 3, 1);

            for (int i = 1; i <= 30; i++, globalIdx++)
            {
                // Usar globalIdx para que la combinación nombre+apellido no se repita entre grupos
                var nombre   = Nombres[globalIdx % Nombres.Length];
                var apellido = Apellidos[(globalIdx / Nombres.Length) % Apellidos.Length];
                var dni      = (dniBase++).ToString();
                var legajo   = $"EST-{prefijo}-{i:D3}";
                var email    = $"alu.{prefijo.ToLower().Replace("-", ".")}.{i:D3}@institucion.edu.ar";

                if (legajosExistentes.Contains(legajo))
                    continue;

                var usuario = Usuario.Crear(
                    dni:          dni,
                    legajo:       legajo,
                    email:        email,
                    nombre:       nombre,
                    apellido:     apellido,
                    passwordHash: passwordHash,
                    rol:          Rol.Estudiante
                );
                db.Usuarios.Add(usuario);
                await db.SaveChangesAsync(ct);

                var estudiante = Estudiante.Crear(
                    usuarioId:      usuario.Id,
                    anio:           grupo.AnioEstudiante,
                    carreraId:      grupo.CarreraId,
                    fechaDeIngreso: fechaIngreso
                );
                db.Estudiantes.Add(estudiante);
                await db.SaveChangesAsync(ct);

                foreach (var materiaId in grupo.MateriaIds)
                {
                    db.InscripcionesMateria.Add(
                        InscripcionMateria.Crear(estudiante.Id, materiaId, grupo.CursoId));
                }
                await db.SaveChangesAsync(ct);

                totalInsertados++;
            }

            logger.LogInformation(
                "EstudiantesSeeder: 30 estudiantes | Carrera {C} | Año {A} | Comisión {Com}",
                grupo.CarreraId, grupo.AnioEstudiante, grupo.Comision);
        }

        logger.LogInformation("EstudiantesSeeder: {T} estudiantes creados en total.", totalInsertados);
    }

    /// <summary>
    /// Corrige nombres duplicados entre alumnos del seed: asigna combinaciones únicas
    /// usando un índice global. Solo modifica Nombre y Apellido; no toca legajos ni emails.
    /// </summary>
    public static async Task FixNombresAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var usuarios = await db.Usuarios
            .Where(u => u.Legajo.StartsWith("EST-"))
            .OrderBy(u => u.Id)
            .Select(u => new { u.Id, u.Nombre, u.Apellido })
            .ToListAsync(ct);

        if (usuarios.Count == 0) return;

        // Detectar si hay duplicados
        bool hayDuplicados = usuarios
            .GroupBy(u => (u.Nombre, u.Apellido))
            .Any(g => g.Count() > 1);

        if (!hayDuplicados)
        {
            logger.LogInformation("EstudiantesSeeder.FixNombres: sin duplicados, nada que corregir.");
            return;
        }

        logger.LogInformation("EstudiantesSeeder.FixNombres: corrigiendo nombres en {N} alumnos...", usuarios.Count);
        int actualizados = 0;

        for (int idx = 0; idx < usuarios.Count; idx++)
        {
            var nombre   = Nombres[idx % Nombres.Length];
            var apellido = Apellidos[(idx / Nombres.Length) % Apellidos.Length];

            if (usuarios[idx].Nombre == nombre && usuarios[idx].Apellido == apellido)
                continue;

            await db.Database.ExecuteSqlRawAsync(
                "UPDATE Usuarios SET Nombre = {0}, Apellido = {1} WHERE Id = {2}",
                nombre, apellido, usuarios[idx].Id);
            actualizados++;
        }

        logger.LogInformation("EstudiantesSeeder.FixNombres: {A} registros actualizados.", actualizados);
    }
}
