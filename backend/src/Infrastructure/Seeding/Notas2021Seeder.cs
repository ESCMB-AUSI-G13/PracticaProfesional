using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.ValueObjects;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Asigna notas a los exámenes del año lectivo 2021.
///
/// Solo afecta a InscripcionesExamen con Estado=Activa (los desertores año 1
/// ya tienen Estado=Baja y no reciben nota).
///
/// Distribución por estudiante (semilla fija para reproducibilidad):
///   20% Promocional  → parciales 7-10, Final = Baja (exento)
///   80% Regular      → parciales 4-6.99, Final aprobado 4-7
///
/// No modifica la condición académica actual del estudiante porque esa
/// condición refleja su trayectoria completa (hasta 2026), no solo 2021.
///
/// Idempotente: si los exámenes de 2021 ya tienen notas, se omite.
/// </summary>
public static class Notas2021Seeder
{
    private const double PctPromocional = 0.20;

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // Guard: si ya hay notas en exámenes de 2021, omitir
        bool yaExisten = await db.InscripcionesExamen
            .AnyAsync(ie => ie.NotaValor != null
                         && db.Examenes
                                .Where(e => e.FechaExamen.Year == 2021)
                                .Select(e => e.Id)
                                .Contains(ie.ExamenId), ct);

        if (yaExisten)
        {
            logger.LogInformation("Notas2021Seeder: las notas ya existen, omitido.");
            return;
        }

        // IDs de exámenes 2021 por tipo
        var examenesIds = await db.Examenes
            .Where(e => e.FechaExamen.Year == 2021)
            .Select(e => new { e.Id, e.TipoExamen, e.MateriaId })
            .ToListAsync(ct);

        if (examenesIds.Count == 0)
        {
            logger.LogWarning("Notas2021Seeder: no hay exámenes de 2021. Ejecutá Examenes2021Seeder primero.");
            return;
        }

        var parcialesIds    = examenesIds.Where(e => e.TipoExamen == TipoExamen.Parcial)
                                         .Select(e => e.Id).ToHashSet();
        var finalesIds      = examenesIds.Where(e => e.TipoExamen == TipoExamen.Final)
                                         .Select(e => e.Id).ToHashSet();

        // Inscripciones activas (los que van a rendir)
        var inscripciones = await db.InscripcionesExamen
            .Where(ie => ie.Estado == EstadoInscripcion.Activa
                      && (parcialesIds.Contains(ie.ExamenId) || finalesIds.Contains(ie.ExamenId)))
            .ToListAsync(ct);

        if (inscripciones.Count == 0)
        {
            logger.LogWarning("Notas2021Seeder: no hay inscripciones activas en 2021.");
            return;
        }

        // Agrupar por estudiante para asignar condición coherente
        var porEstudiante = inscripciones
            .GroupBy(ie => ie.EstudianteId)
            .ToList();

        var rng        = new Random(2021);
        int totalEstudiantes = porEstudiante.Count;
        int limitePromocional = (int)Math.Round(totalEstudiantes * PctPromocional);

        // Mezclar para distribuir aleatoriamente con semilla fija
        var ordenAleatorio = porEstudiante.OrderBy(_ => rng.Next()).ToList();

        int cPromocional = 0, cRegular = 0;

        for (int i = 0; i < ordenAleatorio.Count; i++)
        {
            var grupo       = ordenAleatorio[i];
            bool esPromocional = i < limitePromocional;

            var parciales = grupo.Where(ie => parcialesIds.Contains(ie.ExamenId)).ToList();
            var finales   = grupo.Where(ie => finalesIds.Contains(ie.ExamenId)).ToList();

            if (esPromocional)
            {
                // Notas altas en los 3 parciales (7-10)
                foreach (var ie in parciales)
                    ie.CargarNota(Nota.Crear(Rnd(7m, 10m, rng)));

                // Exento del final
                foreach (var ie in finales)
                    ie.DarDeBaja();

                cPromocional++;
            }
            else
            {
                // Notas regulares en los 3 parciales (4-6.99)
                foreach (var ie in parciales)
                    ie.CargarNota(Nota.Crear(Rnd(4m, 6.99m, rng)));

                // Rinde y aprueba el final (4-7)
                foreach (var ie in finales)
                    ie.CargarNota(Nota.Crear(Rnd(4m, 7m, rng)));

                cRegular++;
            }
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Notas2021Seeder: notas cargadas para {T} estudiantes — {P} Promocionales, {R} Regulares.",
            totalEstudiantes, cPromocional, cRegular);
    }

    private static decimal Rnd(decimal min, decimal max, Random rng)
        => Math.Round(min + (decimal)(rng.NextDouble() * (double)(max - min)), 2);
}
