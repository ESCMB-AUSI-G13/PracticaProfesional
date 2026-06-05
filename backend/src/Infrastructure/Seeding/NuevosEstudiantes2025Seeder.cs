using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Crea los 60 estudiantes Profesorado de la nueva cohorte 2025 (1er año).
///
/// Trayecto 2025 se crea por separado en NuevosEstudiantes2025TrayectoSeeder.
/// Solo se crean Profesorado: 2 comisiones × 30 = 60 alumnos, todos Regular.
///
/// Legajo: EST-H2025-C1{comision}-{n:D3}
/// DNI:    desde 41_008_001
/// Idempotente: si ya existen estudiantes Profesorado 2025, se omite.
/// </summary>
public static class NuevosEstudiantes2025Seeder
{
    private static readonly string[] Nombres =
    [
        "Alejo", "Brenda", "Claudio", "Diana", "Ezequiel", "Fátima",
        "Gonzalo", "Humberto", "Ivana", "Joaquín", "Keila", "Leandro",
        "Mirna", "Néstor", "Ofelia", "Patricio", "Renata", "Santino",
        "Teresa", "Ulrich", "Verónica", "Wilfredo", "Ximena", "Yamila",
        "Zelda", "Anselmo", "Berta", "Conrado", "Dorotea", "Edilberto"
    ];

    private static readonly string[] Apellidos =
    [
        "Almada", "Bonilla", "Cardozo", "Doria", "Escalante", "Fraga",
        "Gallegos", "Hidalgo", "Ibáñez", "Jaimes", "Kopec", "Leal",
        "Machado", "Noriega", "Orellana", "Peral", "Quijano", "Ríos",
        "Salinas", "Tello", "Urbano", "Villalba", "Wolman", "Yunes",
        "Zabala", "Alonso", "Barriga", "Correa", "Dávalos", "Estévez"
    ];

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        bool yaExisten = await db.Estudiantes
            .AnyAsync(e => e.FechaDeIngreso.Year == 2025 && e.CarreraId == 1, ct);

        if (yaExisten)
        {
            logger.LogInformation("NuevosEstudiantes2025Seeder: Profesorado 2025 ya existe, omitido.");
            return;
        }

        var materiaIds = await db.Materias
            .Where(m => m.Anio == 1 && m.CarreraId == 1)
            .OrderBy(m => m.Id)
            .Select(m => m.Id)
            .ToListAsync(ct);

        var cursosPorComision = await db.Cursos
            .Where(c => c.Anio == 2025 && c.AnioLectivo == 1 && c.CarreraId == 1)
            .ToDictionaryAsync(c => c.Comision, c => c.Id, ct);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Alumno2026!");
        var fechaIngreso = new DateTime(2025, 3, 1);
        int dniBase      = 41_008_001;
        int globalIdx    = 0;
        int creados      = 0;

        foreach (var comision in new[] { "A", "B" })
        {
            if (!cursosPorComision.TryGetValue(comision, out var cursoId))
            {
                logger.LogWarning("NuevosEstudiantes2025Seeder: no encontró curso Profesorado Com{C} 2025.", comision);
                continue;
            }

            for (int i = 0; i < 30; i++, globalIdx++)
            {
                var nombre   = Nombres[globalIdx % Nombres.Length];
                var apellido = Apellidos[(globalIdx / Nombres.Length) % Apellidos.Length];
                var legajo   = $"EST-H2025-C1{comision}-{i + 1:D3}";
                var email    = $"alu.h2025.c1{comision.ToLower()}.{i + 1:D3}@institucion.edu.ar";

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
            logger.LogInformation("NuevosEstudiantes2025Seeder: C1{Com} — 30 alumnos creados.", comision);
        }

        var cursosIds = cursosPorComision.Values.ToList();
        await db.InscripcionesMateria
            .Where(i => cursosIds.Contains(i.CursoId))
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.FechaInscripcion, new DateTime(2025, 3, 1)), ct);

        logger.LogInformation("NuevosEstudiantes2025Seeder: {N} estudiantes Profesorado 2025 creados.", creados);
    }
}
