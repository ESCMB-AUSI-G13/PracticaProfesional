using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Crea los 60 estudiantes Profesorado de la nueva cohorte 2023 (1er año).
///
/// Los 60 estudiantes Trayecto 2023 ya existen (creados por CohorteHistoricaSeeder).
/// Solo se crean los de Profesorado: 2 comisiones × 30 = 60 alumnos.
///
/// Legajo: EST-H2023-C1{comision}-{n:D3}
/// DNI:    desde 41_004_001
/// Idempotente: si ya existen estudiantes Profesorado de la cohorte 2023, se omite.
/// </summary>
public static class NuevosEstudiantes2023Seeder
{
    private static readonly string[] Nombres =
    [
        "Agustín", "Belén", "Carlos", "Daniela", "Emilio", "Florencia",
        "Gustavo", "Hernán", "Irina", "Julián", "Karen", "Leonardo",
        "Magdalena", "Nicolás", "Olivia", "Pablo", "Quintina", "Rodrigo",
        "Sabrina", "Tomás", "Ursula", "Vanesa", "Walter", "Ximena",
        "Yésica", "Zacarías", "Andrea", "Braian", "Celeste", "Diego"
    ];

    private static readonly string[] Apellidos =
    [
        "Aguirre", "Bustos", "Contreras", "Dávila", "Echeverría", "Ferreira",
        "Godoy", "Heredia", "Insua", "Juárez", "Kramer", "Leguizamón",
        "Montoya", "Navarro", "Ocampo", "Palacio", "Quiroga", "Roldán",
        "Soldano", "Trujillo", "Ugarte", "Villanueva", "Wenceslao", "Ybarra",
        "Zárate", "Antunez", "Britos", "Cejas", "Doria", "Estrada"
    ];

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // Guard: Trayecto 2023 ya existe; solo verificamos Profesorado (CarreraId=1)
        bool yaExisten = await db.Estudiantes
            .AnyAsync(e => e.FechaDeIngreso.Year == 2023 && e.CarreraId == 1, ct);

        if (yaExisten)
        {
            logger.LogInformation("NuevosEstudiantes2023Seeder: Profesorado 2023 ya existe, omitido.");
            return;
        }

        var materiaIds = await db.Materias
            .Where(m => m.Anio == 1 && m.CarreraId == 1)
            .OrderBy(m => m.Id)
            .Select(m => m.Id)
            .ToListAsync(ct);

        var cursosPorComision = await db.Cursos
            .Where(c => c.Anio == 2023 && c.AnioLectivo == 1 && c.CarreraId == 1)
            .ToDictionaryAsync(c => c.Comision, c => c.Id, ct);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Alumno2026!");
        var fechaIngreso  = new DateTime(2023, 3, 1);
        int dniBase       = 41_004_001;
        int globalIdx     = 0;
        int creados       = 0;

        foreach (var comision in new[] { "A", "B" })
        {
            if (!cursosPorComision.TryGetValue(comision, out var cursoId))
            {
                logger.LogWarning("NuevosEstudiantes2023Seeder: no encontró curso Profesorado Com{C} 2023.", comision);
                continue;
            }

            for (int i = 0; i < 30; i++, globalIdx++)
            {
                var nombre   = Nombres[globalIdx % Nombres.Length];
                var apellido = Apellidos[(globalIdx / Nombres.Length) % Apellidos.Length];
                var legajo   = $"EST-H2023-C1{comision}-{i + 1:D3}";
                var email    = $"alu.h2023.c1{comision.ToLower()}.{i + 1:D3}@institucion.edu.ar";

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
            logger.LogInformation("NuevosEstudiantes2023Seeder: C1{Com} — 30 alumnos creados.", comision);
        }

        var cursosIds = cursosPorComision.Values.ToList();
        await db.InscripcionesMateria
            .Where(i => cursosIds.Contains(i.CursoId))
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.FechaInscripcion, new DateTime(2023, 3, 1)), ct);

        logger.LogInformation("NuevosEstudiantes2023Seeder: {N} estudiantes Profesorado 2023 creados.", creados);
    }
}
