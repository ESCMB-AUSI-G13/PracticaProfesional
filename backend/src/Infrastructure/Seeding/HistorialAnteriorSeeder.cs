using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Genera registros de HistorialAcademico para los años calendario intermedios
/// (2024 y 2025) que el seed principal no cubrió, permitiendo que el reporte
/// RR-12 muestre tasas de retención longitudinales coherentes.
///
/// Tasas simuladas (fijas con semilla 42 para reproducibilidad):
///   Cohorte 2023 → Año 2 (2024): ~92% | Año 3 (2025): ~88%
///   Cohorte 2024 → Año 2 (2025): ~93%
/// </summary>
public static class HistorialAnteriorSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        if (await db.HistorialAcademico.AnyAsync(h => h.Anio == 2024 || h.Anio == 2025, ct))
        {
            logger.LogInformation("HistorialAnteriorSeeder: registros históricos ya existentes, omitido.");
            return;
        }

        // Necesitamos un (MateriaId, CursoId) válido para satisfacer los FK.
        // Usamos el primero disponible — solo importa el campo Anio para el reporte.
        var primerCursoId   = await db.Cursos.Select(c => c.Id).FirstOrDefaultAsync(ct);
        var primerMateriaId = await db.Materias.Select(m => m.Id).FirstOrDefaultAsync(ct);

        if (primerCursoId == 0 || primerMateriaId == 0)
        {
            logger.LogWarning("HistorialAnteriorSeeder: sin Cursos o Materias disponibles, omitido.");
            return;
        }

        var est2023 = await db.Estudiantes
            .Where(e => e.FechaDeIngreso.Year == 2023)
            .Select(e => e.Id)
            .ToListAsync(ct);

        var est2024 = await db.Estudiantes
            .Where(e => e.FechaDeIngreso.Year == 2024)
            .Select(e => e.Id)
            .ToListAsync(ct);

        if (est2023.Count == 0 && est2024.Count == 0)
        {
            logger.LogWarning("HistorialAnteriorSeeder: sin estudiantes de cohorte 2023/2024, omitido.");
            return;
        }

        var rng     = new Random(42);
        var records = new List<HistorialAcademico>();

        // Cohorte 2023 — deben tener historial en 2024 (Año 2) y 2025 (Año 3)
        foreach (var id in est2023)
        {
            if (rng.NextDouble() < 0.92)
                records.Add(HistorialAcademico.Crear(id, primerMateriaId, primerCursoId,
                    2024, "A", "Aprobado", Math.Round((decimal)(rng.NextDouble() * 3 + 5), 1),
                    CondicionEstudiante.Regular));

            if (rng.NextDouble() < 0.88)
                records.Add(HistorialAcademico.Crear(id, primerMateriaId, primerCursoId,
                    2025, "A", "Aprobado", Math.Round((decimal)(rng.NextDouble() * 3 + 5), 1),
                    CondicionEstudiante.Regular));
        }

        // Cohorte 2024 — deben tener historial en 2025 (Año 2)
        foreach (var id in est2024)
        {
            if (rng.NextDouble() < 0.93)
                records.Add(HistorialAcademico.Crear(id, primerMateriaId, primerCursoId,
                    2025, "A", "Aprobado", Math.Round((decimal)(rng.NextDouble() * 3 + 5), 1),
                    CondicionEstudiante.Regular));
        }

        db.HistorialAcademico.AddRange(records);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "HistorialAnteriorSeeder: {N} registros históricos creados (cohorte 2023: {A}, cohorte 2024: {B}).",
            records.Count, est2023.Count, est2024.Count);
    }
}
