using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Crea los 60 estudiantes Profesorado de la nueva cohorte 2026 (1er año).
///
/// Trayecto 2026 se crea por separado en NuevosEstudiantes2026TrayectoSeeder.
/// Solo se crean Profesorado: 2 comisiones × 30 = 60 alumnos, todos Regular.
///
/// Legajo: EST-H2026-C1{comision}-{n:D3}
/// DNI:    desde 41_010_001
/// Idempotente: si ya existen estudiantes Profesorado 2026, se omite.
/// </summary>
public static class NuevosEstudiantes2026Seeder
{
    private static readonly string[] Nombres =
    [
        "Adrián", "Belinda", "César", "Dinora", "Enrique", "Flora",
        "Germán", "Horacio", "Iris", "Jenaro", "Karla", "Luca",
        "Mara", "Nadia", "Osman", "Paula", "Quentin", "Rita",
        "Sofía", "Tobías", "Ulises", "Viola", "Wilson", "Xenia",
        "Yanira", "Zara", "Agustín", "Bárbara", "Carlos", "Delia"
    ];

    private static readonly string[] Apellidos =
    [
        "Alarcón", "Barrera", "Campos", "Durán", "Espinosa", "Flores",
        "Guerrero", "Hechavarría", "Ibarguen", "Jerez", "Kazuki", "Llanos",
        "Maldonado", "Nieves", "Olivares", "Pizarro", "Quintana", "Ruiz",
        "Sarabia", "Trejo", "Uribe", "Valenzuela", "Wicca", "Yáñez",
        "Zapata", "Aguado", "Blanco", "Cruzat", "Daza", "Elizalde"
    ];

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        bool yaExisten = await db.Estudiantes
            .AnyAsync(e => e.FechaDeIngreso.Year == 2026 && e.CarreraId == 1, ct);

        if (yaExisten)
        {
            logger.LogInformation("NuevosEstudiantes2026Seeder: Profesorado 2026 ya existe, omitido.");
            return;
        }

        var materiaIds = await db.Materias
            .Where(m => m.Anio == 1 && m.CarreraId == 1)
            .OrderBy(m => m.Id)
            .Select(m => m.Id)
            .ToListAsync(ct);

        var cursosPorComision = await db.Cursos
            .Where(c => c.Anio == 2026 && c.AnioLectivo == 1 && c.CarreraId == 1)
            .ToDictionaryAsync(c => c.Comision, c => c.Id, ct);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Alumno2026!");
        var fechaIngreso = new DateTime(2026, 3, 1);
        int dniBase      = 41_010_001;
        int globalIdx    = 0;
        int creados      = 0;

        foreach (var comision in new[] { "A", "B" })
        {
            if (!cursosPorComision.TryGetValue(comision, out var cursoId))
            {
                logger.LogWarning("NuevosEstudiantes2026Seeder: no encontró curso Profesorado Com{C} 2026.", comision);
                continue;
            }

            for (int i = 0; i < 30; i++, globalIdx++)
            {
                var nombre   = Nombres[globalIdx % Nombres.Length];
                var apellido = Apellidos[(globalIdx / Nombres.Length) % Apellidos.Length];
                var legajo   = $"EST-H2026-C1{comision}-{i + 1:D3}";
                var email    = $"alu.h2026.c1{comision.ToLower()}.{i + 1:D3}@institucion.edu.ar";

                var usuario = Usuario.Crear(
                    (dniBase++).ToString(), legajo, email,
                    nombre, apellido, passwordHash, Rol.Estudiante);
                db.Usuarios.Add(usuario);
                await db.SaveChangesAsync(ct);

                var estudiante = Estudiante.Crear(usuario.Id, anio: 1, carreraId: 1, fechaIngreso);
                db.Estudiantes.Add(estudiante);
                await db.SaveChangesAsync(ct);

                foreach (var materiaId in materiaIds)
                    db.InscripcionesMateria.Add(InscripcionMateria.Crear(estudiante.Id, materiaId, cursoId));

                creados++;
            }

            await db.SaveChangesAsync(ct);
            logger.LogInformation("NuevosEstudiantes2026Seeder: C1{Com} — 30 alumnos creados.", comision);
        }

        var cursosIds = cursosPorComision.Values.ToList();
        await db.InscripcionesMateria
            .Where(i => cursosIds.Contains(i.CursoId))
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.FechaInscripcion, new DateTime(2026, 3, 1)), ct);

        logger.LogInformation("NuevosEstudiantes2026Seeder: {N} estudiantes Profesorado 2026 creados.", creados);
    }
}
