using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Genera registros de asistencia para la cohorte 2021 (1er año).
///
/// Tasas por condición académica:
///   Promocional  → 90-98%  (muy alta asistencia)
///   Regular      → 75-90%  (asistencia suficiente)
///   Libre        → 50-74%  (baja asistencia → causa de quedar Libre)
///   Desertor     → 70-85%  durante el período que cursaron,
///                  luego sin registros (abandonaron)
///
/// Período de clases 2021:
///   01-mar al 30-nov, lunes a viernes,
///   sin feriados nacionales ni receso invernal (12-23 jul).
///
/// Idempotente: si ya existen asistencias de 2021, se omite.
/// Guarda en batches de 500 registros para minimizar round-trips.
/// </summary>
public static class Asistencias2021Seeder
{
    private const int BatchSize = 500;

    private static readonly string[] MotivosJustificados =
    [
        "Certificado médico", "Enfermedad", "Motivo familiar",
        "Trámite laboral", "Turno médico", "Motivo personal"
    ];

    // Feriados nacionales Argentina 2021 + receso invernal
    private static readonly HashSet<DateTime> DiasNoLectivos = new()
    {
        new(2021,  3, 24), // Día de la Memoria
        new(2021,  4,  1), // Jueves Santo
        new(2021,  4,  2), // Viernes Santo / Veteranos
        new(2021,  5, 25), // Revolución de Mayo
        new(2021,  6, 21), // Día de la Bandera (trasladado)
        new(2021,  7,  9), // Independencia
        // Receso invernal: 12-23 julio
        new(2021,  7, 12), new(2021,  7, 13), new(2021,  7, 14),
        new(2021,  7, 15), new(2021,  7, 16), new(2021,  7, 19),
        new(2021,  7, 20), new(2021,  7, 21), new(2021,  7, 22),
        new(2021,  7, 23),
        new(2021,  8, 16), // San Martín (trasladado)
        new(2021, 10, 11), // Diversidad Cultural (trasladado)
        new(2021, 11, 22), // Soberanía Nacional (trasladado)
    };

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // Guard
        bool yaExisten = await db.Asistencias
            .AnyAsync(a => a.Fecha.Year == 2021, ct);

        if (yaExisten)
        {
            logger.LogInformation("Asistencias2021Seeder: ya existen registros de 2021, omitido.");
            return;
        }

        // Cursos de 2021
        var cursosIds2021 = await db.Cursos
            .Where(c => c.Anio == 2021 && c.AnioLectivo == 1)
            .Select(c => c.Id)
            .ToListAsync(ct);

        if (cursosIds2021.Count == 0)
        {
            logger.LogWarning("Asistencias2021Seeder: sin cursos de 2021.");
            return;
        }

        // Todas las inscripciones a materias de 2021 (activos y desertores)
        var inscripciones = await db.InscripcionesMateria
            .Where(im => cursosIds2021.Contains(im.CursoId))
            .Select(im => new { im.EstudianteId, im.MateriaId, im.CursoId, im.Estado })
            .ToListAsync(ct);

        if (inscripciones.Count == 0)
        {
            logger.LogWarning("Asistencias2021Seeder: sin inscripciones de 2021.");
            return;
        }

        // Condición por estudiante
        var condicionPorEstudiante = await db.Estudiantes
            .Where(e => e.FechaDeIngreso.Year == 2021)
            .Select(e => new { e.Id, e.Condicion })
            .ToDictionaryAsync(e => e.Id, e => e.Condicion, ct);

        // Días lectivos del año 2021
        var diasLectivos = GenerarDiasLectivos2021();
        logger.LogInformation("Asistencias2021Seeder: {D} días lectivos en 2021.", diasLectivos.Count);

        var rng   = new Random(2021);
        var batch = new List<Asistencia>(BatchSize);
        int total = 0;

        foreach (var insc in inscripciones)
        {
            if (!condicionPorEstudiante.TryGetValue(insc.EstudianteId, out var condicion))
                continue;

            // Determinar hasta qué día cursó (desertores salen antes)
            DateTime? fechaAbandon = condicion == CondicionEstudiante.Desertor
                ? GenerarFechaAbandon(rng)
                : null;

            foreach (var dia in diasLectivos)
            {
                // Desertor: sin registros después de que abandonó
                if (fechaAbandon.HasValue && dia > fechaAbandon.Value)
                    break;

                var estado = GenerarEstado(condicion, rng);
                var motivo = estado == EstadoAsistencia.AusenteJustificado
                    ? MotivosJustificados[rng.Next(MotivosJustificados.Length)]
                    : null;

                batch.Add(Asistencia.Registrar(
                    insc.EstudianteId, insc.MateriaId, insc.CursoId,
                    dia, estado, motivo));

                if (batch.Count >= BatchSize)
                {
                    db.Asistencias.AddRange(batch);
                    await db.SaveChangesAsync(ct);
                    total += batch.Count;
                    batch.Clear();
                }
            }
        }

        if (batch.Count > 0)
        {
            db.Asistencias.AddRange(batch);
            await db.SaveChangesAsync(ct);
            total += batch.Count;
        }

        logger.LogInformation(
            "Asistencias2021Seeder: {T} registros creados para {E} inscripciones.",
            total, inscripciones.Count);
    }

    private static List<DateTime> GenerarDiasLectivos2021()
    {
        var dias  = new List<DateTime>();
        var desde = new DateTime(2021, 3, 1);
        var hasta = new DateTime(2021, 11, 30);

        for (var d = desde; d <= hasta; d = d.AddDays(1))
        {
            if (d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;
            if (DiasNoLectivos.Contains(d)) continue;
            dias.Add(d);
        }
        return dias;
    }

    // Fecha aleatoria entre abril y agosto en que el desertor abandonó
    private static DateTime GenerarFechaAbandon(Random rng)
    {
        var desde = new DateTime(2021, 4, 1);
        var hasta = new DateTime(2021, 8, 31);
        int dias  = (int)(hasta - desde).TotalDays;
        return desde.AddDays(rng.Next(dias));
    }

    private static EstadoAsistencia GenerarEstado(CondicionEstudiante condicion, Random rng)
    {
        double r = rng.NextDouble();
        return condicion switch
        {
            CondicionEstudiante.Promocional =>
                r < 0.93 ? EstadoAsistencia.Presente
                : r < 0.98 ? EstadoAsistencia.AusenteJustificado
                : EstadoAsistencia.Ausente,

            CondicionEstudiante.Regular =>
                r < 0.82 ? EstadoAsistencia.Presente
                : r < 0.91 ? EstadoAsistencia.AusenteJustificado
                : EstadoAsistencia.Ausente,

            CondicionEstudiante.Libre =>
                r < 0.60 ? EstadoAsistencia.Presente
                : r < 0.72 ? EstadoAsistencia.AusenteJustificado
                : EstadoAsistencia.Ausente,

            CondicionEstudiante.Desertor =>
                r < 0.75 ? EstadoAsistencia.Presente
                : r < 0.87 ? EstadoAsistencia.AusenteJustificado
                : EstadoAsistencia.Ausente,

            _ => EstadoAsistencia.Presente
        };
    }
}
