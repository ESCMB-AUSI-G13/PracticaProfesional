using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Crea registros de HistorialAcademico para la cohorte 2021 al cierre del año lectivo.
///
/// EstadoFinal y NotaFinal según condición:
///   Promocional → "Aprobado",  nota = promedio de parciales (7-10)
///   Regular     → "Aprobado",  nota = nota del examen final (4-7)
///   Libre       → "Libre",     nota = promedio de parciales (1-3.99)
///   Desertor    → "Abandono",  nota = null
///
/// Idempotente: si ya existen registros de historial para 2021, se omite.
/// </summary>
public static class HistorialAcademico2021Seeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // Guard
        bool yaExisten = await db.HistorialAcademico
            .AnyAsync(h => h.Anio == 2021, ct);

        if (yaExisten)
        {
            logger.LogInformation("HistorialAcademico2021Seeder: ya existen registros de 2021, omitido.");
            return;
        }

        // Cursos e inscripciones de 2021
        var cursosIds2021 = await db.Cursos
            .Where(c => c.Anio == 2021 && c.AnioLectivo == 1)
            .Select(c => c.Id)
            .ToListAsync(ct);

        if (cursosIds2021.Count == 0)
        {
            logger.LogWarning("HistorialAcademico2021Seeder: sin cursos de 2021.");
            return;
        }

        // Inscripciones a materias con su comisión (vía Curso)
        var inscripciones = await db.InscripcionesMateria
            .Where(im => cursosIds2021.Contains(im.CursoId))
            .Select(im => new
            {
                im.EstudianteId,
                im.MateriaId,
                im.CursoId,
                Comision = db.Cursos
                    .Where(c => c.Id == im.CursoId)
                    .Select(c => c.Comision)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        // Condición por estudiante
        var estudiantes = await db.Estudiantes
            .Where(e => e.FechaDeIngreso.Year == 2021)
            .Select(e => new { e.Id, e.Condicion })
            .ToDictionaryAsync(e => e.Id, e => e.Condicion, ct);

        // Notas de parciales por (EstudianteId, MateriaId) — promedio
        var examenesParcialesIds = await db.Examenes
            .Where(e => e.FechaExamen.Year == 2021 && e.TipoExamen == TipoExamen.Parcial)
            .Select(e => new { e.Id, e.MateriaId })
            .ToListAsync(ct);

        var examenFinalesIds = await db.Examenes
            .Where(e => e.FechaExamen.Year == 2021 && e.TipoExamen == TipoExamen.Final)
            .Select(e => new { e.Id, e.MateriaId })
            .ToListAsync(ct);

        // Notas de parciales: (EstudianteId, MateriaId) → promedio
        var parcialesExamenIds = examenesParcialesIds.Select(e => e.Id).ToHashSet();
        var finalesExamenIds   = examenFinalesIds.Select(e => e.Id).ToHashSet();

        var notasParciales = await db.InscripcionesExamen
            .Where(ie => parcialesExamenIds.Contains(ie.ExamenId) && ie.NotaValor != null)
            .Select(ie => new { ie.EstudianteId, ie.ExamenId, ie.NotaValor })
            .ToListAsync(ct);

        var notasFinales = await db.InscripcionesExamen
            .Where(ie => finalesExamenIds.Contains(ie.ExamenId) && ie.NotaValor != null)
            .Select(ie => new { ie.EstudianteId, ie.ExamenId, ie.NotaValor })
            .ToListAsync(ct);

        // Mapa examenId → materiaId
        var parcialMateriaMap = examenesParcialesIds.ToDictionary(e => e.Id, e => e.MateriaId);
        var finalMateriaMap   = examenFinalesIds.ToDictionary(e => e.Id, e => e.MateriaId);

        // Promedio parciales por (EstudianteId, MateriaId)
        var promParcialesPorEstMat = notasParciales
            .GroupBy(n => (n.EstudianteId, parcialMateriaMap.GetValueOrDefault(n.ExamenId)))
            .ToDictionary(g => g.Key, g => (decimal)g.Average(n => (double)n.NotaValor!.Value));

        // Nota final por (EstudianteId, MateriaId)
        var notaFinalPorEstMat = notasFinales
            .GroupBy(n => (n.EstudianteId, finalMateriaMap.GetValueOrDefault(n.ExamenId)))
            .ToDictionary(g => g.Key, g => g.Max(n => n.NotaValor!.Value));

        // Crear registros de HistorialAcademico
        var registros = new List<HistorialAcademico>();

        foreach (var insc in inscripciones)
        {
            if (!estudiantes.TryGetValue(insc.EstudianteId, out var condicion))
                continue;

            var comision = insc.Comision ?? "A";
            var key      = (insc.EstudianteId, insc.MateriaId);

            string estadoFinal;
            decimal? notaFinal;

            switch (condicion)
            {
                case CondicionEstudiante.Promocional:
                    estadoFinal = "Promocional";
                    notaFinal   = promParcialesPorEstMat.GetValueOrDefault(key);
                    break;

                case CondicionEstudiante.Regular:
                    estadoFinal = "Regularizado";
                    notaFinal   = notaFinalPorEstMat.ContainsKey(key)
                        ? notaFinalPorEstMat[key]
                        : promParcialesPorEstMat.GetValueOrDefault(key);
                    break;

                case CondicionEstudiante.Libre:
                    estadoFinal = "Libre";
                    notaFinal   = promParcialesPorEstMat.GetValueOrDefault(key);
                    break;

                case CondicionEstudiante.Desertor:
                    estadoFinal = "Abandonó";
                    notaFinal   = null;
                    break;

                default:
                    estadoFinal = "Regularizado";
                    notaFinal   = promParcialesPorEstMat.GetValueOrDefault(key);
                    break;
            }

            registros.Add(HistorialAcademico.Crear(
                insc.EstudianteId, insc.MateriaId, insc.CursoId,
                2021, comision, estadoFinal, notaFinal, condicion));
        }

        // Guardar en batches de 500
        const int batchSize = 500;
        for (int i = 0; i < registros.Count; i += batchSize)
        {
            db.HistorialAcademico.AddRange(registros.Skip(i).Take(batchSize));
            await db.SaveChangesAsync(ct);
        }

        logger.LogInformation(
            "HistorialAcademico2021Seeder: {N} registros creados para {E} inscripciones.",
            registros.Count, inscripciones.Count);
    }
}
