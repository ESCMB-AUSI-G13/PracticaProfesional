using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

public static class ExamenesSeeder
{
    private static readonly string[] Horarios = ["08:00-10:00", "11:00-13:00", "14:00-16:00", "17:00-19:00"];

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var materias = await db.Materias.AsNoTracking().ToListAsync(ct);
        if (materias.Count == 0)
        {
            logger.LogWarning("ExamenesSeeder: no hay materias en la BD, seed omitido.");
            return;
        }

        // Solo crear examen para materias que todavía no tienen ninguno
        var materiasConExamen = (await db.Examenes
            .Select(e => e.MateriaId)
            .Distinct()
            .ToListAsync(ct))
            .ToHashSet();

        var materiasSinExamen = materias
            .Where(m => !materiasConExamen.Contains(m.Id))
            .ToList();

        if (materiasSinExamen.Count == 0)
        {
            logger.LogInformation("ExamenesSeeder: todas las materias ya tienen examen, seed omitido.");
            return;
        }

        var baseDate = DateTime.UtcNow.Date;
        var examenes = new List<Examen>();

        for (int i = 0; i < materiasSinExamen.Count; i++)
        {
            var fecha   = baseDate.AddDays(i % 5);
            var horario = Horarios[i % Horarios.Length];
            var examen  = Examen.Crear(materiasSinExamen[i].Id, fecha, horario, 30, TipoExamen.Parcial);
            db.Examenes.Add(examen);
            examenes.Add(examen);
        }

        await db.SaveChangesAsync(ct);

        // Auto-inscripción para los exámenes nuevos
        var materiaIdToExamenId = examenes.ToDictionary(e => e.MateriaId, e => e.Id);

        var inscripcionesActivas = await db.InscripcionesMateria
            .Where(im => im.Estado == EstadoInscripcion.Activa
                      && materiaIdToExamenId.Keys.Contains(im.MateriaId))
            .ToListAsync(ct);

        // Evitar duplicados si ya hay inscripciones examen previas para estos exámenes
        var examenIds = examenes.Select(e => e.Id).ToHashSet();
        var yaInscritos = (await db.InscripcionesExamen
            .Where(ie => examenIds.Contains(ie.ExamenId))
            .Select(ie => new { ie.EstudianteId, ie.ExamenId })
            .ToListAsync(ct))
            .Select(x => (x.EstudianteId, x.ExamenId))
            .ToHashSet();

        var inscripcionesNuevas = inscripcionesActivas
            .Select(im => (im.EstudianteId, ExamenId: materiaIdToExamenId[im.MateriaId]))
            .DistinctBy(x => (x.EstudianteId, x.ExamenId))
            .Where(x => !yaInscritos.Contains(x))
            .Select(x => InscripcionExamen.Crear(x.EstudianteId, x.ExamenId))
            .ToList();

        if (inscripcionesNuevas.Count > 0)
        {
            db.InscripcionesExamen.AddRange(inscripcionesNuevas);
            await db.SaveChangesAsync(ct);
        }

        logger.LogInformation(
            "ExamenesSeeder: {E} exámenes y {I} inscripciones creados.",
            examenes.Count, inscripcionesNuevas.Count);
    }
}
