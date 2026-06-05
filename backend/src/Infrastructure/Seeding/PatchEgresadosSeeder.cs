using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Corrige las distribuciones de egresados para que el reporte "Egresados por Carrera"
/// refleje tasas históricamente consistentes:
///
///   Trayecto (carreraId=2):
///     2021: 8  → 15 egresados (+7)   | 25%   | duración promedio ~3.2 años (mayor)
///     2022: 19 → 17 egresados (-2)   | 28%   | duración promedio ~2.7 años
///     2023: 26 → 18 egresados (-8)   | 30%   | duración promedio ~2.4 años
///     2024: 26 → 15 egresados (-11)  | 25%   | duración promedio ~1.8 años (menor)
///     2025:  N → 0  egresados        | —     | cohorte no completada aún
///     2026:  N → 0  egresados        | —     | cohorte no completada aún
///
///   Profesorado (carreraId=1):
///     2021:  0 → 10 egresados (+10)  | 17%   | duración ~4.7 años
///     2022:  0 →  8 egresados (+8)   | 13%   | duración ~4.1 años
///
///   Total Trayecto visible: 65/240 ≈ 27% global ✓
///
/// FechaDeEgreso redistribuida para que la duración promedio decrezca
/// desde 2021 hacia 2024 (los de 2021 tuvieron más tiempo para egresar).
///
/// Idempotente: se omite si Profesorado 2021 ya tiene ≥ 8 egresados.
/// </summary>
public static class PatchEgresadosSeeder
{
    public static async Task PatchAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // Guard: si Profesorado 2021 ya tiene egresados, el patch ya se aplicó
        int egresadosProf2021 = await db.Estudiantes.CountAsync(
            e => e.CarreraId == 1
              && e.FechaDeIngreso.Year == 2021
              && e.Condicion == CondicionEstudiante.Egresado, ct);

        if (egresadosProf2021 >= 8)
        {
            logger.LogInformation("PatchEgresadosSeeder: ya aplicado, omitido.");
            return;
        }

        var rng = new Random(2026);

        // ── 1. Trayecto 2025 y 2026: revertir TODOS los egresados ────────────────
        // No pueden haber egresados en cohortes que no completaron los 2 años del plan.
        await RevertirTodosEgresados(db, logger, carreraId: 2, cohorte: 2025, ct);
        await RevertirTodosEgresados(db, logger, carreraId: 2, cohorte: 2026, ct);

        // ── 2. Trayecto 2021: agregar 7 egresados (8 → 15, 25%) ─────────────────
        await AgregarEgresados(db, carreraId: 2, cohorte: 2021, cantidad: 7, anioFinal: 2, ct);

        // ── 3. Trayecto 2022: revertir 2 egresados (19 → 17, 28%) ───────────────
        await RevertirEgresados(db, carreraId: 2, cohorte: 2022, cantidad: 2, ct);

        // ── 4. Trayecto 2023: revertir 8 egresados (26 → 18, 30%) ───────────────
        await RevertirEgresados(db, carreraId: 2, cohorte: 2023, cantidad: 8, ct);

        // ── 5. Trayecto 2024: revertir 11 egresados (26 → 15, 25%) ──────────────
        await RevertirEgresados(db, carreraId: 2, cohorte: 2024, cantidad: 11, ct);

        // ── 6. Redistribuir FechaDeEgreso Trayecto (duración: 2021 > 2022 > 2023 > 2024) ─
        await RedistribuirFechasTrayecto(db, rng, ct);

        // ── 7. Profesorado 2021: agregar 10 egresados (0 → 10, 17%) ─────────────
        // Carrera de 4 años: cohorte 2021 completó en 2025.
        await AgregarEgresados(db, carreraId: 1, cohorte: 2021, cantidad: 10, anioFinal: 4, ct);
        await AsignarFechasNuevosEgresados(db, carreraId: 1, cohorte: 2021,
            desde: new DateTime(2025, 6, 1), hasta: new DateTime(2026, 3, 31), rng, ct);

        // ── 8. Profesorado 2022: agregar 8 egresados (0 → 8, 13%) ───────────────
        // Carrera de 4 años: cohorte 2022 completó en 2026 (algunos aún terminando).
        await AgregarEgresados(db, carreraId: 1, cohorte: 2022, cantidad: 8, anioFinal: 4, ct);
        await AsignarFechasNuevosEgresados(db, carreraId: 1, cohorte: 2022,
            desde: new DateTime(2026, 3, 1), hasta: new DateTime(2026, 5, 31), rng, ct);

        logger.LogInformation(
            "PatchEgresadosSeeder: completado — Trayecto 2021-2024 ajustado a 25-30%, " +
            "Profesorado 2021 (+10) y 2022 (+8) con egresados. " +
            "Total Trayecto visible: ~65/240 = 27%.");
    }

    // ── Revertir TODOS los egresados de una cohorte que no debería tenerlos ──
    private static async Task RevertirTodosEgresados(
        AppDbContext db, ILogger logger, int carreraId, int cohorte, CancellationToken ct)
    {
        var ids = await db.Estudiantes
            .Where(e => e.CarreraId == carreraId
                     && e.FechaDeIngreso.Year == cohorte
                     && e.Condicion == CondicionEstudiante.Egresado)
            .Select(e => e.Id)
            .ToListAsync(ct);

        if (ids.Count == 0) return;

        foreach (var id in ids)
        {
            await db.Database.ExecuteSqlRawAsync(
                "UPDATE Estudiantes SET Condicion = 'Regular', FechaDeEgreso = NULL WHERE Id = {0}", id);
        }

        logger.LogInformation(
            "PatchEgresadosSeeder: {N} egresados Trayecto {C} revertidos a Regular (no deben existir aún).",
            ids.Count, cohorte);
    }

    // ── Revertir N egresados (los de mayor Id, selección determinista) ───────
    private static async Task RevertirEgresados(
        AppDbContext db, int carreraId, int cohorte, int cantidad, CancellationToken ct)
    {
        var ids = await db.Estudiantes
            .Where(e => e.CarreraId == carreraId
                     && e.FechaDeIngreso.Year == cohorte
                     && e.Condicion == CondicionEstudiante.Egresado)
            .OrderByDescending(e => e.Id)
            .Take(cantidad)
            .Select(e => e.Id)
            .ToListAsync(ct);

        foreach (var id in ids)
        {
            await db.Database.ExecuteSqlRawAsync(
                "UPDATE Estudiantes SET Condicion = 'Regular', FechaDeEgreso = NULL WHERE Id = {0}", id);
        }
    }

    // ── Agregar N egresados desde estudiantes Regular (los de menor Id) ───────
    private static async Task AgregarEgresados(
        AppDbContext db, int carreraId, int cohorte, int cantidad, int anioFinal, CancellationToken ct)
    {
        var ids = await db.Estudiantes
            .Where(e => e.CarreraId == carreraId
                     && e.FechaDeIngreso.Year == cohorte
                     && e.Condicion == CondicionEstudiante.Regular)
            .OrderBy(e => e.Id)
            .Take(cantidad)
            .Select(e => e.Id)
            .ToListAsync(ct);

        foreach (var id in ids)
        {
            await db.Database.ExecuteSqlRawAsync(
                $"UPDATE Estudiantes SET Condicion = 'Egresado', Anio = {anioFinal}, FechaDeEgreso = NULL WHERE Id = {{0}}",
                id);
        }
    }

    // ── Asignar FechaDeEgreso a egresados sin fecha (recién convertidos) ─────
    private static async Task AsignarFechasNuevosEgresados(
        AppDbContext db, int carreraId, int cohorte,
        DateTime desde, DateTime hasta, Random rng, CancellationToken ct)
    {
        var ids = await db.Estudiantes
            .Where(e => e.CarreraId == carreraId
                     && e.FechaDeIngreso.Year == cohorte
                     && e.Condicion == CondicionEstudiante.Egresado
                     && e.FechaDeEgreso == null)
            .Select(e => e.Id)
            .ToListAsync(ct);

        int rango = Math.Max(1, (hasta - desde).Days);
        foreach (var id in ids)
        {
            var fecha = desde.AddDays(rng.Next(rango));
            await db.Database.ExecuteSqlRawAsync(
                "UPDATE Estudiantes SET FechaDeEgreso = {0} WHERE Id = {1}", fecha, id);
        }
    }

    // ── Redistribuir FechaDeEgreso Trayecto ──────────────────────────────────
    // Objetivo: duración promedio decreciente desde 2021 hacia 2024.
    //   2021 ≈ 3.2 años (muchos tomaron más tiempo, primera cohorte del plan)
    //   2022 ≈ 2.7 años
    //   2023 ≈ 2.4 años
    //   2024 ≈ 1.8 años (egresaron en el tiempo nominal)
    private static async Task RedistribuirFechasTrayecto(AppDbContext db, Random rng, CancellationToken ct)
    {
        var rangos = new[]
        {
            // 2021 (ingreso 2021-03): range oct 2023 – sep 2025 → avg ≈ may 2024 → ~3.2 años
            (cohorte: 2021, desde: new DateTime(2023, 10, 1), hasta: new DateTime(2025, 9, 30)),
            // 2022 (ingreso 2022-03): range jul 2024 – oct 2025 → avg ≈ feb 2025 → ~2.9 años
            (cohorte: 2022, desde: new DateTime(2024, 7, 1),  hasta: new DateTime(2025, 10, 31)),
            // 2023 (ingreso 2023-03): range may 2025 – ene 2026 → avg ≈ sep 2025 → ~2.5 años
            (cohorte: 2023, desde: new DateTime(2025, 5, 1),  hasta: new DateTime(2026, 1, 31)),
            // 2024 (ingreso 2024-03): range oct 2025 – may 2026 → avg ≈ feb 2026 → ~1.9 años
            (cohorte: 2024, desde: new DateTime(2025, 10, 1), hasta: new DateTime(2026, 5, 31)),
        };

        foreach (var r in rangos)
        {
            var ids = await db.Estudiantes
                .Where(e => e.CarreraId == 2
                         && e.FechaDeIngreso.Year == r.cohorte
                         && e.Condicion == CondicionEstudiante.Egresado)
                .Select(e => e.Id)
                .ToListAsync(ct);

            int rango = Math.Max(1, (r.hasta - r.desde).Days);
            foreach (var id in ids)
            {
                var fecha = r.desde.AddDays(rng.Next(rango));
                await db.Database.ExecuteSqlRawAsync(
                    "UPDATE Estudiantes SET FechaDeEgreso = {0} WHERE Id = {1}", fecha, id);
            }
        }
    }
}
