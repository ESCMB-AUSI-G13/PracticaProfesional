using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Crea los exámenes del año lectivo 2021 (1er año de cada carrera).
///
/// 4 instancias por materia:
///   1er Parcial — mayo 2021
///   2do Parcial — agosto 2021
///   3er Parcial — octubre 2021
///   Final       — diciembre 2021
///
/// Inscripciones:
///   InscripcionMateria Aprobada → InscripcionExamen Activa  (rindieron)
///   InscripcionMateria Baja     → InscripcionExamen Baja    (desertores, no rindieron)
///
/// Idempotente: si ya existen exámenes de 2021, se omite.
/// </summary>
public static class Examenes2021Seeder
{
    private static readonly string[] Horarios = ["08:00-10:00", "11:00-13:00", "14:00-16:00", "17:00-19:00"];

    private static readonly (TipoExamen Tipo, DateTime[] Fechas)[] Instancias =
    [
        (TipoExamen.Parcial, [
            new(2021, 5,  3), new(2021, 5,  4), new(2021, 5,  5),
            new(2021, 5,  6), new(2021, 5,  7), new(2021, 5, 10),
            new(2021, 5, 11), new(2021, 5, 12)
        ]),
        (TipoExamen.Parcial, [
            new(2021, 8,  2), new(2021, 8,  3), new(2021, 8,  4),
            new(2021, 8,  5), new(2021, 8,  6), new(2021, 8,  9),
            new(2021, 8, 10), new(2021, 8, 11)
        ]),
        (TipoExamen.Parcial, [
            new(2021, 10,  4), new(2021, 10,  5), new(2021, 10,  6),
            new(2021, 10,  7), new(2021, 10,  8), new(2021, 10, 11),
            new(2021, 10, 12), new(2021, 10, 13)
        ]),
        (TipoExamen.Final, [
            new(2021, 12,  6), new(2021, 12,  7), new(2021, 12,  8),
            new(2021, 12,  9), new(2021, 12, 10), new(2021, 12, 13),
            new(2021, 12, 14), new(2021, 12, 15)
        ]),
    ];

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        bool yaExisten = await db.Examenes
            .AnyAsync(e => e.FechaExamen.Year == 2021, ct);

        if (yaExisten)
        {
            logger.LogInformation("Examenes2021Seeder: ya existen exámenes de 2021, omitido.");
            return;
        }

        var cursoIds2021 = await db.Cursos
            .Where(c => c.Anio == 2021 && c.AnioLectivo == 1)
            .Select(c => c.Id)
            .ToListAsync(ct);

        if (cursoIds2021.Count == 0)
        {
            logger.LogWarning("Examenes2021Seeder: no hay cursos de 2021.");
            return;
        }

        var materiaIds = await db.InscripcionesMateria
            .Where(im => cursoIds2021.Contains(im.CursoId))
            .Select(im => im.MateriaId)
            .Distinct()
            .OrderBy(id => id)
            .ToListAsync(ct);

        if (materiaIds.Count == 0)
        {
            logger.LogWarning("Examenes2021Seeder: no hay materias inscriptas en 2021.");
            return;
        }

        var inscripcionesMateria = await db.InscripcionesMateria
            .Where(im => cursoIds2021.Contains(im.CursoId))
            .Select(im => new { im.EstudianteId, im.MateriaId, im.Estado })
            .ToListAsync(ct);

        var todosLosExamenes   = new List<Examen>();
        var todasInscripciones = new List<InscripcionExamen>();

        for (int instIdx = 0; instIdx < Instancias.Length; instIdx++)
        {
            var (tipo, fechas) = Instancias[instIdx];

            for (int mIdx = 0; mIdx < materiaIds.Count; mIdx++)
            {
                var fecha   = fechas[mIdx % fechas.Length];
                var horario = Horarios[mIdx % Horarios.Length];
                var examen  = Examen.CrearHistorico(materiaIds[mIdx], fecha, horario, cupo: 30, tipo);
                db.Examenes.Add(examen);
                todosLosExamenes.Add(examen);
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation(
            "Examenes2021Seeder: {E} exámenes creados ({M} materias × 4 instancias).",
            todosLosExamenes.Count, materiaIds.Count);

        // Mapa (materiaId, instanciaIdx) → examenId
        var mapaExamen = new Dictionary<(int MateriaId, int InstanciaIdx), int>();
        for (int instIdx = 0; instIdx < Instancias.Length; instIdx++)
            for (int mIdx = 0; mIdx < materiaIds.Count; mIdx++)
                mapaExamen[(materiaIds[mIdx], instIdx)] = todosLosExamenes[instIdx * materiaIds.Count + mIdx].Id;

        var yaInscritos = new HashSet<(int EstudianteId, int ExamenId)>();

        foreach (var im in inscripcionesMateria)
        {
            for (int instIdx = 0; instIdx < Instancias.Length; instIdx++)
            {
                if (!mapaExamen.TryGetValue((im.MateriaId, instIdx), out var examenId))
                    continue;

                if (!yaInscritos.Add((im.EstudianteId, examenId)))
                    continue;

                var ie = InscripcionExamen.Crear(im.EstudianteId, examenId);

                if (im.Estado == EstadoInscripcion.Baja)
                    ie.DarDeBaja();

                todasInscripciones.Add(ie);
            }
        }

        db.InscripcionesExamen.AddRange(todasInscripciones);
        await db.SaveChangesAsync(ct);

        var examenIds = todosLosExamenes.Select(e => e.Id).ToList();
        await db.InscripcionesExamen
            .Where(ie => examenIds.Contains(ie.ExamenId))
            .ExecuteUpdateAsync(s => s.SetProperty(ie => ie.FechaInscripcion, new DateTime(2021, 3, 1)), ct);

        logger.LogInformation(
            "Examenes2021Seeder: {T} inscripciones — {A} activas, {B} baja.",
            todasInscripciones.Count,
            todasInscripciones.Count(ie => ie.Estado == EstadoInscripcion.Activa),
            todasInscripciones.Count(ie => ie.Estado == EstadoInscripcion.Baja));
    }
}
