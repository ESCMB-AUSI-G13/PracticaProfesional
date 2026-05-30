using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Crea todos los Cursos académicos para los años 2021-2026.
///
/// Lógica de cursos por año:
///   Cada cohorte ingresa en año lectivo 1 y avanza un nivel por año.
///   Profesorado (CarreraId=1): dura 4 años → cohorte 2021 cubre años lectivos 1-4 en 2021-2024.
///   Trayecto   (CarreraId=2): dura 2 años → cohorte 2021 cubre años lectivos 1-2 en 2021-2022.
///
/// Cursos creados por año calendario:
///   2021: Prof 1, Tray 1                                  (comisiones A+B cada uno → 4 cursos)
///   2022: Prof 1-2, Tray 1-2                              → 8 cursos
///   2023: Prof 1-3, Tray 1-2                              → 10 cursos
///   2024: Prof 1-4, Tray 1-2                              → 12 cursos
///   2025: Prof 1-4, Tray 1-2                              → 12 cursos
///   2026: Prof 1-4, Tray 1-2                              → 12 cursos
///   Total: 58 cursos
///
/// Idempotente: omite cualquier curso que ya exista por (Anio, AnioLectivo, Comision, CarreraId).
/// PreceptorId: se asigna en round-robin entre los preceptores disponibles en la BD.
/// </summary>
public static class CursosSeeder
{
    private static readonly string[] Comisiones = ["A", "B"];

    // (AnioCalendario, AnioLectivo, CarreraId)
    private static IEnumerable<(int Anio, int AnioLectivo, int CarreraId)> GenerarCombinaciones()
    {
        // Años calendario cubiertos
        for (int anio = 2021; anio <= 2026; anio++)
        {
            // Profesorado: anioLectivo activo = desde 1 hasta min(anio-2021+1, 4)
            int maxProfesoral = Math.Min(anio - 2021 + 1, 4);
            for (int nivel = 1; nivel <= maxProfesoral; nivel++)
                yield return (anio, nivel, 1);

            // Trayecto: anioLectivo activo = desde 1 hasta min(anio-2021+1, 2)
            int maxTrayecto = Math.Min(anio - 2021 + 1, 2);
            for (int nivel = 1; nivel <= maxTrayecto; nivel++)
                yield return (anio, nivel, 2);
        }
    }

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var preceptorIds = await db.Preceptores
            .OrderBy(p => p.Id)
            .Select(p => p.Id)
            .ToListAsync(ct);

        if (preceptorIds.Count == 0)
        {
            logger.LogWarning("CursosSeeder: no hay preceptores en la BD, seed omitido.");
            return;
        }

        var existentes = await db.Cursos
            .Select(c => new { c.Anio, c.AnioLectivo, c.Comision, c.CarreraId })
            .ToListAsync(ct);

        var claves = existentes
            .Select(c => (c.Anio, c.AnioLectivo, c.Comision, c.CarreraId))
            .ToHashSet();

        int creados = 0;
        int preceptorIdx = 0;

        foreach (var (anio, anioLectivo, carreraId) in GenerarCombinaciones())
        {
            foreach (var comision in Comisiones)
            {
                if (claves.Contains((anio, anioLectivo, comision, carreraId)))
                    continue;

                var preceptorId = preceptorIds[preceptorIdx % preceptorIds.Count];
                preceptorIdx++;

                var curso = Curso.Crear(anio, anioLectivo, comision, cupo: 30, preceptorId, carreraId);
                if (anio < DateTime.UtcNow.Year)
                    curso.Cerrar();
                db.Cursos.Add(curso);
                claves.Add((anio, anioLectivo, comision, carreraId));
                creados++;
            }
        }

        if (creados > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("CursosSeeder: {N} cursos creados (2021-2026).", creados);
        }
        else
        {
            logger.LogInformation("CursosSeeder: todos los cursos ya existen, omitido.");
        }
    }

    /// <summary>
    /// Devuelve el Id del curso que corresponde a una combinación dada.
    /// Lanza si no existe (el CursosSeeder debe ejecutarse antes).
    /// </summary>
    public static async Task<int> ObtenerIdAsync(
        AppDbContext db,
        int anio, int anioLectivo, string comision, int carreraId,
        CancellationToken ct = default)
    {
        var id = await db.Cursos
            .Where(c => c.Anio == anio
                     && c.AnioLectivo == anioLectivo
                     && c.Comision == comision.ToUpperInvariant()
                     && c.CarreraId == carreraId)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(ct);

        if (id == 0)
            throw new InvalidOperationException(
                $"No existe el curso (Anio={anio}, AnioLectivo={anioLectivo}, Comision={comision}, CarreraId={carreraId}). Ejecutá CursosSeeder primero.");

        return id;
    }
}
