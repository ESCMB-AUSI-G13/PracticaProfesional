using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Corrige la deserción en cohortes recientes y históricas:
///
/// Problema: NuevosEstudiantes2025Seeder y 2026Seeder crearon todos los alumnos
/// como Regular. SeedDesertoresActivosAsync agregó "fantasmas" (sin inscripciones)
/// que no aparecen en el denominador del reporte, causando 0% en esos años.
///
/// Solución:
///   1. Elimina los fantasmas EST-D (sin registros académicos → sesgan el numerador).
///   2. Prof 2025 — convierte 20 Regulares en Desertores reales:
///        - 14 deserción temprana : asistencias hasta julio, sin exámenes → Baja
///        - 6  deserción por riesgo: asistencias con muchas ausencias + notas 1-3 → Baja
///   3. Prof 2026 — convierte 8 Regulares en Desertores tempranos (~13%).
///   4. Prof 2021 y 2022 — agrega 2 Desertores de Año 4 por cohorte (faltaban).
///
/// Idempotente: se omite si Prof 2025 ya tiene desertores.
/// </summary>
public static class PatchDesercionSeeder
{
    public static async Task PatchAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        bool yaAplicado = await db.Estudiantes.AnyAsync(
            e => e.CarreraId == 1 && e.FechaDeIngreso.Year == 2025
              && e.Condicion == CondicionEstudiante.Desertor, ct);

        if (yaAplicado)
        {
            logger.LogInformation("PatchDesercionSeeder: ya aplicado, omitido.");
            return;
        }

        await EliminarFantasmas(db, logger, ct);
        await ConvertirDesercionProf2025(db, logger, ct);
        await ConvertirDesercionProf2026(db, logger, ct);
        await AgregarDesertoresAnio4(db, logger, ct);

        logger.LogInformation("PatchDesercionSeeder: completado.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1. Eliminar fantasmas EST-D (no tienen inscripciones ni historial real)
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task EliminarFantasmas(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        var prefijos = new[] { "EST-D2023-C1", "EST-D2024-C1", "EST-D2025-C1", "EST-D2025-C2" };

        int totalElim = 0;
        foreach (var prefijo in prefijos)
        {
            var usuarioIds = await db.Usuarios
                .Where(u => u.Legajo.StartsWith(prefijo))
                .Select(u => u.Id)
                .ToListAsync(ct);

            if (usuarioIds.Count == 0) continue;

            var estIds = await db.Estudiantes
                .Where(e => usuarioIds.Contains(e.UsuarioId))
                .Select(e => e.Id)
                .ToListAsync(ct);

            if (estIds.Count > 0)
            {
                await db.HistorialAcademico.Where(h => estIds.Contains(h.EstudianteId)).ExecuteDeleteAsync(ct);
                await db.InscripcionesMateria.Where(im => estIds.Contains(im.EstudianteId)).ExecuteDeleteAsync(ct);
                await db.Asistencias.Where(a => estIds.Contains(a.EstudianteId)).ExecuteDeleteAsync(ct);
                await db.Estudiantes.Where(e => estIds.Contains(e.Id)).ExecuteDeleteAsync(ct);
            }
            await db.Usuarios.Where(u => usuarioIds.Contains(u.Id)).ExecuteDeleteAsync(ct);

            totalElim += usuarioIds.Count;
            logger.LogInformation("PatchDesercionSeeder.Fantasmas: {N} eliminados ({P}).", usuarioIds.Count, prefijo);
        }

        logger.LogInformation("PatchDesercionSeeder.Fantasmas: {T} fantasmas eliminados en total.", totalElim);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. Prof 2025 — 14 deserción temprana + 6 deserción por riesgo
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task ConvertirDesercionProf2025(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        var regulares = await db.Estudiantes
            .Where(e => e.CarreraId == 1 && e.FechaDeIngreso.Year == 2025
                     && e.Condicion == CondicionEstudiante.Regular)
            .OrderBy(e => e.Id)
            .Take(20)
            .Select(e => e.Id)
            .ToListAsync(ct);

        if (regulares.Count == 0)
        {
            logger.LogWarning("PatchDesercionSeeder.Prof2025: sin Regulares para convertir.");
            return;
        }

        var temprana  = regulares.Take(14).ToList();
        var porRiesgo = regulares.Skip(14).ToList();

        // ── Deserción temprana: abandonaron antes de julio (sin segundo cuatrimestre)
        var corte2025 = new DateTime(2025, 7, 1);

        await db.Asistencias
            .Where(a => temprana.Contains(a.EstudianteId) && a.Fecha >= corte2025)
            .ExecuteDeleteAsync(ct);

        await db.InscripcionesExamen
            .Where(ie => temprana.Contains(ie.EstudianteId))
            .ExecuteDeleteAsync(ct);

        if (temprana.Count > 0)
            await db.Database.ExecuteSqlRawAsync(
                $"UPDATE HistorialAcademico SET EstadoFinal = 'Abandonó', NotaFinal = NULL " +
                $"WHERE EstudianteId IN ({string.Join(",", temprana)})");

        await db.InscripcionesMateria
            .Where(im => temprana.Contains(im.EstudianteId))
            .ExecuteUpdateAsync(s => s.SetProperty(im => im.Estado, EstadoInscripcion.Baja), ct);

        await db.Estudiantes
            .Where(e => temprana.Contains(e.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.Condicion, CondicionEstudiante.Desertor)
                .SetProperty(e => e.Anio, 1), ct);

        // ── Deserción por riesgo: asistieron pero con malas notas y muchas ausencias
        var mitad2025 = new DateTime(2025, 6, 1);

        // Segunda mitad del año → todo Ausente
        await db.Asistencias
            .Where(a => porRiesgo.Contains(a.EstudianteId) && a.Fecha >= mitad2025)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Estado, EstadoAsistencia.Ausente), ct);

        // Bajar notas de exámenes a 1-3
        var examenIds = await (
            from ie in db.InscripcionesExamen
            join ex in db.Examenes on ie.ExamenId equals ex.Id
            where porRiesgo.Contains(ie.EstudianteId) && ie.NotaValor.HasValue
                  && ex.TipoExamen != TipoExamen.Final
            select ie.Id
        ).ToListAsync(ct);

        foreach (var ieId in examenIds)
            await db.Database.ExecuteSqlRawAsync(
                "UPDATE InscripcionesExamen SET NotaValor = ((Id % 3) + 1) WHERE Id = {0}", ieId);

        // Eliminar inscripción al examen Final (no llegaron)
        await (
            from ie in db.InscripcionesExamen
            join ex in db.Examenes on ie.ExamenId equals ex.Id
            where porRiesgo.Contains(ie.EstudianteId) && ex.TipoExamen == TipoExamen.Final
            select ie
        ).ExecuteDeleteAsync(ct);

        if (porRiesgo.Count > 0)
            await db.Database.ExecuteSqlRawAsync(
                $"UPDATE HistorialAcademico SET EstadoFinal = 'Abandonó', NotaFinal = NULL " +
                $"WHERE EstudianteId IN ({string.Join(",", porRiesgo)})");

        await db.InscripcionesMateria
            .Where(im => porRiesgo.Contains(im.EstudianteId))
            .ExecuteUpdateAsync(s => s.SetProperty(im => im.Estado, EstadoInscripcion.Baja), ct);

        await db.Estudiantes
            .Where(e => porRiesgo.Contains(e.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.Condicion, CondicionEstudiante.Desertor)
                .SetProperty(e => e.Anio, 1), ct);

        logger.LogInformation(
            "PatchDesercionSeeder.Prof2025: {T} desertores ({D1} tempranos, {D2} por riesgo).",
            temprana.Count + porRiesgo.Count, temprana.Count, porRiesgo.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3. Prof 2026 — 8 desertores tempranos (año en curso, ~13%)
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task ConvertirDesercionProf2026(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        var regulares = await db.Estudiantes
            .Where(e => e.CarreraId == 1 && e.FechaDeIngreso.Year == 2026
                     && e.Condicion == CondicionEstudiante.Regular)
            .OrderBy(e => e.Id)
            .Take(8)
            .Select(e => e.Id)
            .ToListAsync(ct);

        if (regulares.Count == 0)
        {
            logger.LogWarning("PatchDesercionSeeder.Prof2026: sin Regulares para convertir.");
            return;
        }

        // Abandono muy temprano: solo asistieron las primeras 3 clases (marzo-abril)
        var corte2026 = new DateTime(2026, 5, 1);

        await db.Asistencias
            .Where(a => regulares.Contains(a.EstudianteId) && a.Fecha >= corte2026)
            .ExecuteDeleteAsync(ct);

        await db.InscripcionesExamen
            .Where(ie => regulares.Contains(ie.EstudianteId))
            .ExecuteDeleteAsync(ct);

        if (regulares.Count > 0)
            await db.Database.ExecuteSqlRawAsync(
                $"UPDATE HistorialAcademico SET EstadoFinal = 'Abandonó', NotaFinal = NULL " +
                $"WHERE EstudianteId IN ({string.Join(",", regulares)})");

        await db.InscripcionesMateria
            .Where(im => regulares.Contains(im.EstudianteId))
            .ExecuteUpdateAsync(s => s.SetProperty(im => im.Estado, EstadoInscripcion.Baja), ct);

        await db.Estudiantes
            .Where(e => regulares.Contains(e.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.Condicion, CondicionEstudiante.Desertor)
                .SetProperty(e => e.Anio, 1), ct);

        logger.LogInformation("PatchDesercionSeeder.Prof2026: {N} desertores tempranos.", regulares.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 4. Prof 2021 y 2022 — 2 desertores de Año 4 por cohorte (faltaban)
    //    Son Regulares que completaron los primeros 3 años pero no terminaron la carrera
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task AgregarDesertoresAnio4(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        foreach (var (cohorte, cantidad) in new[] { (2021, 2), (2022, 2) })
        {
            var ids = await db.Estudiantes
                .Where(e => e.CarreraId == 1
                         && e.FechaDeIngreso.Year == cohorte
                         && e.Condicion == CondicionEstudiante.Regular
                         && e.Anio == 4)
                .OrderByDescending(e => e.Id)
                .Take(cantidad)
                .Select(e => e.Id)
                .ToListAsync(ct);

            if (ids.Count == 0)
            {
                logger.LogWarning(
                    "PatchDesercionSeeder.Anio4 {C}: sin Regulares Año 4, omitido.", cohorte);
                continue;
            }

            // Solo cambia condición — mantienen todos sus registros académicos intactos
            // (abandonaron al final, no en el medio del año)
            await db.Estudiantes
                .Where(e => ids.Contains(e.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.Condicion, CondicionEstudiante.Desertor)
                    .SetProperty(e => e.Anio, 4), ct);

            logger.LogInformation(
                "PatchDesercionSeeder.Anio4 {C}: {N} desertores Año 4 agregados.", cohorte, ids.Count);
        }
    }
}
