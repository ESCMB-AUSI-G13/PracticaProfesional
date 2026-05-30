using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.ValueObjects;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Genera exámenes e inscripciones con notas para cohortes históricas 2021-2025.
///
/// Fechas de examen:
///   Años 2021-2025 → noviembre del año académico (fin de cursada).
///   Año 2026       → mayo 2026 (límite del ciclo actual).
///
/// Notas por condición:
///   Egresado/Promocional : aprobado 7-10 en todos los años.
///   Regular              : aprobado 4-7 en años previos; terminal: 60% aprobado, 30% reprobado, 10% ausente.
///   Libre                : aprobado 4-6 en años previos; terminal: 30% aprobado, 50% reprobado, 20% ausente.
///   Desertor             : años previos mezclados; año de deserción: 55% ausente, 45% reprobado.
///   EST-C activos        : solo años anteriores al actual → aprobado (avanzaron de año).
/// </summary>
public static class NotasHistoricasSeeder
{
    private static readonly string[] Horarios =
        ["08:00-10:00", "11:00-13:00", "14:00-16:00", "17:00-19:00"];

    private static readonly DateTime LimiteMayo2026 = new(2026, 5, 15);

    // IDs hardcodeados porque el campo Anio en BD no está correctamente poblado.
    private static readonly Dictionary<(int CarreraId, int Anio), int[]> MateriasPorAnio = new()
    {
        [(1, 1)] = [17, 18, 19, 20, 21, 22, 23, 24, 48],
        [(1, 2)] = [25, 26, 27, 28, 29, 30, 31, 49, 50],
        [(1, 3)] = [32, 33, 34, 35, 36, 37, 38, 39, 51],
        [(1, 4)] = [40, 41, 42, 43, 44, 45, 46, 47, 52, 53],
        [(2, 1)] = [4, 6, 7, 8, 9, 10, 11],
        [(2, 2)] = [12, 13, 14, 15, 16],
    };

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // Guard: si ya existen exámenes históricos (anteriores a 2026), omitir.
        bool yaExisten = await db.Examenes.AnyAsync(e => e.FechaExamen.Year < 2026, ct);
        if (yaExisten)
        {
            logger.LogInformation("NotasHistoricas: exámenes históricos ya existen, omitido.");
            return;
        }

        var rng = new Random(53);

        // Estudiantes de cohortes 2021-2025 (históricos + activos con años previos)
        var estudiantes = await db.Estudiantes
            .Include(e => e.Usuario)
            .Where(e => e.FechaDeIngreso.Year >= 2021 && e.FechaDeIngreso.Year <= 2025)
            .ToListAsync(ct);

        if (estudiantes.Count == 0)
        {
            logger.LogWarning("NotasHistoricas: sin estudiantes 2021-2025, omitido.");
            return;
        }

        // Cache compartido: (materiaId, anioExamen) → examenId
        var cache = new Dictionary<(int, int), int>();
        int totalExamenes = 0, totalInscripciones = 0;

        foreach (var est in estudiantes)
        {
            int cohorte    = est.FechaDeIngreso.Year;
            int anioActual = est.Anio;

            // Activos (EST-C): ExamenesSeeder ya maneja su año actual → solo crear años previos.
            bool esActivo = est.Usuario.Legajo.StartsWith("EST-C");
            int  anioMax  = esActivo ? anioActual - 1 : anioActual;

            for (int anioMateria = 1; anioMateria <= anioMax; anioMateria++)
            {
                if (!MateriasPorAnio.TryGetValue((est.CarreraId, anioMateria), out var matIds))
                    continue;

                int      anioExamen = cohorte + anioMateria - 1;
                if (anioExamen > 2026) continue;

                DateTime fechaExamen = anioExamen < 2026
                    ? new DateTime(anioExamen, 11, 15)
                    : LimiteMayo2026;

                // ¿Es el año en que desertó?
                bool esAnioDesercion = est.Condicion == CondicionEstudiante.Desertor
                                    && anioMateria == anioActual;

                // ¿Es un año previo que tuvo que aprobar para avanzar?
                bool esPriorAprobado = !esAnioDesercion
                                    && anioMateria < anioActual
                                    && est.Condicion != CondicionEstudiante.Desertor;

                // EST-C activos: todos sus años son previos → aprobados
                if (esActivo) esPriorAprobado = true;

                foreach (var materiaId in matIds)
                {
                    // Obtener o crear examen histórico compartido
                    if (!cache.TryGetValue((materiaId, anioExamen), out var examenId))
                    {
                        var examen = Examen.Crear(
                            materiaId, fechaExamen,
                            Horarios[rng.Next(Horarios.Length)],
                            cupo: 40, TipoExamen.Final);
                        db.Examenes.Add(examen);
                        await db.SaveChangesAsync(ct);
                        examenId = examen.Id;
                        cache[(materiaId, anioExamen)] = examenId;
                        totalExamenes++;
                    }

                    var inscripcion = InscripcionExamen.Crear(est.Id, examenId);
                    AsignarNota(inscripcion, est.Condicion, esPriorAprobado, esAnioDesercion, rng);
                    db.InscripcionesExamen.Add(inscripcion);
                    totalInscripciones++;
                }

                // Guardar cada 400 inscripciones para no acumular demasiado en memoria
                if (totalInscripciones % 400 == 0)
                    await db.SaveChangesAsync(ct);
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation(
            "NotasHistoricas: {E} exámenes y {I} inscripciones creados para {S} estudiantes.",
            totalExamenes, totalInscripciones, estudiantes.Count);
    }

    // ── Asignación de nota según reglas de negocio ────────────────────────────
    private static void AsignarNota(
        InscripcionExamen ie,
        CondicionEstudiante condicion,
        bool esPriorAprobado,
        bool esAnioDesercion,
        Random rng)
    {
        // Año de deserción: ausente o reprobado casi siempre
        if (esAnioDesercion)
        {
            if (rng.NextDouble() < 0.55) ie.DarDeBaja();
            else                         ie.CargarNota(Nota.Crear(Rnd(1m, 3.5m, rng)));
            return;
        }

        // Años previos que el alumno aprobó para avanzar de año
        if (esPriorAprobado)
        {
            ie.CargarNota(Nota.Crear(Rnd(4.5m, 8.5m, rng)));
            return;
        }

        // Año terminal según condición académica
        switch (condicion)
        {
            case CondicionEstudiante.Egresado:
                ie.CargarNota(Nota.Crear(Rnd(7m, 10m, rng)));
                break;

            case CondicionEstudiante.Promocional:
                ie.CargarNota(Nota.Crear(Rnd(8m, 10m, rng)));
                break;

            case CondicionEstudiante.Regular:
            {
                var p = rng.NextDouble();
                if      (p < 0.10) ie.DarDeBaja();
                else if (p < 0.30) ie.CargarNota(Nota.Crear(Rnd(1m, 3.9m, rng)));
                else               ie.CargarNota(Nota.Crear(Rnd(4m, 7m, rng)));
                break;
            }

            case CondicionEstudiante.Libre:
            {
                var p = rng.NextDouble();
                if      (p < 0.20) ie.DarDeBaja();
                else if (p < 0.70) ie.CargarNota(Nota.Crear(Rnd(1m, 3.9m, rng)));
                else               ie.CargarNota(Nota.Crear(Rnd(4m, 5.5m, rng)));
                break;
            }

            case CondicionEstudiante.Desertor:
            {
                var p = rng.NextDouble();
                if      (p < 0.30) ie.DarDeBaja();
                else if (p < 0.75) ie.CargarNota(Nota.Crear(Rnd(1m, 3.9m, rng)));
                else               ie.CargarNota(Nota.Crear(Rnd(4m, 6m, rng)));
                break;
            }
        }
    }

    private static decimal Rnd(decimal min, decimal max, Random rng)
        => Math.Min(10m, Math.Round(min + (decimal)(rng.NextDouble() * (double)(max - min)), 1));
}
