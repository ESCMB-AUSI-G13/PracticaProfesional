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
        if (await db.Examenes.AnyAsync(ct))
        {
            logger.LogInformation("ExamenesSeeder: ya existen exámenes, seed omitido.");
            return;
        }

        var materias = await db.Materias.AsNoTracking().ToListAsync(ct);
        if (materias.Count == 0)
        {
            logger.LogWarning("ExamenesSeeder: no hay materias en la BD, seed omitido.");
            return;
        }

        var baseDate = DateTime.UtcNow.Date;
        var examenes = new List<Examen>();

        for (int i = 0; i < materias.Count; i++)
        {
            var fecha   = baseDate.AddDays(i % 5);       // distribuye en 5 días (hoy + 0..4)
            var horario = Horarios[i % Horarios.Length];
            var examen  = Examen.Crear(materias[i].Id, fecha, horario, 30, TipoExamen.Parcial);
            db.Examenes.Add(examen);
            examenes.Add(examen);
        }

        await db.SaveChangesAsync(ct);

        // Auto-inscripción: igual a lo que hace CrearExamenUseCase para Parcial
        var inscripcionesActivas = await db.InscripcionesMateria
            .Where(im => im.Estado == EstadoInscripcion.Activa)
            .ToListAsync(ct);

        var materiaIdToExamenId = examenes.ToDictionary(e => e.MateriaId, e => e.Id);

        var inscripcionesExamen = inscripcionesActivas
            .Where(im => materiaIdToExamenId.ContainsKey(im.MateriaId))
            .Select(im => (im.EstudianteId, ExamenId: materiaIdToExamenId[im.MateriaId]))
            .DistinctBy(x => (x.EstudianteId, x.ExamenId))
            .Select(x => InscripcionExamen.Crear(x.EstudianteId, x.ExamenId))
            .ToList();

        if (inscripcionesExamen.Count > 0)
        {
            db.InscripcionesExamen.AddRange(inscripcionesExamen);
            await db.SaveChangesAsync(ct);
        }

        logger.LogInformation(
            "ExamenesSeeder: {E} exámenes y {I} inscripciones de examen creados.",
            examenes.Count, inscripcionesExamen.Count);
    }
}
