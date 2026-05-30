using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Crea los EspaciosCurriculares del año lectivo 2021 (1er año).
///
/// Asigna un docente por materia (el mismo docente enseña la misma materia
/// en las comisiones A y B de su carrera → 16 materias × 2 comisiones = 32 registros).
///
/// Distribución: docentes asignados en round-robin según disponibilidad en BD.
/// Idempotente: si ya existen EspaciosCurriculares para cursos de 2021, se omite.
/// </summary>
public static class EspaciosCurriculares2021Seeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // Guard
        var cursosIds2021 = await db.Cursos
            .Where(c => c.Anio == 2021 && c.AnioLectivo == 1)
            .Select(c => c.Id)
            .ToListAsync(ct);

        if (cursosIds2021.Count == 0)
        {
            logger.LogWarning("EspaciosCurriculares2021Seeder: sin cursos de 2021.");
            return;
        }

        bool yaExisten = await db.EspaciosCurriculares
            .AnyAsync(ec => cursosIds2021.Contains(ec.CursoId), ct);

        if (yaExisten)
        {
            logger.LogInformation("EspaciosCurriculares2021Seeder: ya existen, omitido.");
            return;
        }

        // Cursos de 2021 indexados por (CarreraId, Comision)
        var cursos = await db.Cursos
            .Where(c => c.Anio == 2021 && c.AnioLectivo == 1)
            .ToListAsync(ct);

        // Materias de 1er año agrupadas por carrera
        var materiasPorCarrera = await db.Materias
            .Where(m => m.Anio == 1 && (m.CarreraId == 1 || m.CarreraId == 2))
            .GroupBy(m => m.CarreraId)
            .ToDictionaryAsync(g => g.Key, g => g.OrderBy(m => m.Id).ToList(), ct);

        // Docentes disponibles (round-robin)
        var docenteIds = await db.Docentes
            .OrderBy(d => d.Id)
            .Select(d => d.Id)
            .ToListAsync(ct);

        if (docenteIds.Count == 0)
        {
            logger.LogWarning("EspaciosCurriculares2021Seeder: sin docentes en la BD.");
            return;
        }

        // Mapa fijo materia → docente (mismo docente en ambas comisiones)
        var docentePorMateria = new Dictionary<int, int>();
        int idx = 0;
        foreach (var (_, materias) in materiasPorCarrera)
            foreach (var materia in materias)
                docentePorMateria[materia.Id] = docenteIds[idx++ % docenteIds.Count];

        // Crear EspaciosCurriculares
        var nuevos = new List<EspacioCurricular>();

        foreach (var curso in cursos)
        {
            if (!materiasPorCarrera.TryGetValue(curso.CarreraId, out var materiasCarrera))
                continue;

            foreach (var materia in materiasCarrera)
            {
                var docenteId = docentePorMateria[materia.Id];
                nuevos.Add(EspacioCurricular.Crear(materia.Id, docenteId, curso.Id));
            }
        }

        db.EspaciosCurriculares.AddRange(nuevos);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "EspaciosCurriculares2021Seeder: {N} espacios creados para {C} cursos.",
            nuevos.Count, cursos.Count);
    }
}
