using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.ValueObjects;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Redistribuye la condición académica de alumnos activos para que el tablero
/// ejecutivo refleje distribuciones estadísticamente realistas:
///   Regular ~57% · Libre ~29% · Promocional ~14%
///
/// Por cada estudiante reconvertido actualiza coherentemente:
///   1. Estudiante.Condicion          (vía máquina de estados del dominio)
///   2. InscripcionesExamen.NotaValor (exámenes del año 2024)
///   3. HistorialAcademico 2024       (EstadoFinal, NotaFinal, Condicion)
///   4. InscripcionMateria.Estado     (cursos 2024; Libre → Desaprobada)
///   5. Asistencias 2024              (Libre → sólo primeras 9 de 18 como Presente)
///
/// Selección determinista sobre Regular ordenados por Id:
///   posición % 5  == 0 → Libre
///   posición % 15 == 0 → Promocional  (sobre los que no pasaron a Libre)
///
/// Idempotente: se omite si Libre ya supera el 20 % de activos.
/// </summary>
public static class CondicionRealistaSeeder
{
    private const int AnioTarget = 2024;

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // ── Guard de idempotencia ────────────────────────────────────────────
        int totalActivos = await db.Estudiantes.CountAsync(
            e => e.Condicion != CondicionEstudiante.Egresado
              && e.Condicion != CondicionEstudiante.Desertor, ct);

        int totalLibres = await db.Estudiantes.CountAsync(
            e => e.Condicion == CondicionEstudiante.Libre, ct);

        if (totalActivos > 0 && (decimal)totalLibres / totalActivos >= 0.20m)
        {
            logger.LogInformation(
                "CondicionRealistaSeeder: Libre ya es {P:P0} ({L}/{T}), omitido.",
                (decimal)totalLibres / totalActivos, totalLibres, totalActivos);
            return;
        }

        // ── 1. Selección determinista ─────────────────────────────────────────
        var regulares = await db.Estudiantes
            .Where(e => e.Condicion == CondicionEstudiante.Regular)
            .OrderBy(e => e.Id)
            .ToListAsync(ct);

        var aLibre = regulares
            .Select((e, i) => (e, idx: i + 1))
            .Where(x => x.idx % 5 == 0)
            .Select(x => x.e)
            .ToList();

        var libresSet = aLibre.Select(e => e.Id).ToHashSet();

        var aPromo = regulares
            .Where(e => !libresSet.Contains(e.Id))
            .Select((e, i) => (e, idx: i + 1))
            .Where(x => x.idx % 15 == 0)
            .Select(x => x.e)
            .ToList();

        var promosSet = aPromo.Select(e => e.Id).ToHashSet();
        var todosSet  = libresSet.Union(promosSet).ToHashSet();

        logger.LogInformation(
            "CondicionRealistaSeeder: seleccionados {L} → Libre, {P} → Promocional.",
            aLibre.Count, aPromo.Count);

        // ── 2. Estudiante.Condicion (máquina de estados) ─────────────────────
        foreach (var e in aLibre) e.PerderRegularidad();
        foreach (var e in aPromo) e.ObtenerPromocion();
        await db.SaveChangesAsync(ct);
        logger.LogInformation("CondicionRealistaSeeder: Condicion actualizada.");

        // ── 3. InscripcionesExamen 2024 ───────────────────────────────────────
        // RectificarNota requiere Estado Aprobada/Desaprobada, que es lo que tienen.
        var inscExamen = await db.InscripcionesExamen
            .Include(ie => ie.Examen)
            .Where(ie => todosSet.Contains(ie.EstudianteId)
                      && ie.Examen.FechaExamen.Year == AnioTarget
                      && (ie.Estado == EstadoInscripcion.Aprobada
                       || ie.Estado == EstadoInscripcion.Desaprobada))
            .ToListAsync(ct);

        foreach (var ie in inscExamen)
        {
            var cond = libresSet.Contains(ie.EstudianteId)
                ? CondicionEstudiante.Libre
                : CondicionEstudiante.Promocional;

            ie.RectificarNota(Nota.Crear(
                GenerarNota(cond, ie.EstudianteId, ie.ExamenId, ie.Examen.TipoExamen)));
        }
        await db.SaveChangesAsync(ct);
        logger.LogInformation("CondicionRealistaSeeder: InscripcionesExamen actualizadas ({N}).", inscExamen.Count);

        // ── 4. HistorialAcademico 2024 ────────────────────────────────────────
        // ExecuteUpdateAsync para saltear los private setters de la entidad.
        // NotaFinal usa la misma fórmula que EstadoFinal() pero expresada en SQL.

        var libresListIds = libresSet.ToList();
        var promosListIds = promosSet.ToList();

        await db.HistorialAcademico
            .Where(h => h.Anio == AnioTarget && libresListIds.Contains(h.EstudianteId))
            .ExecuteUpdateAsync(s => s
                .SetProperty(h => h.EstadoFinal, "Libre")
                .SetProperty(h => h.NotaFinal,   h => (decimal?)(1 + h.EstudianteId % 3))
                .SetProperty(h => h.Condicion,   CondicionEstudiante.Libre), ct);

        await db.HistorialAcademico
            .Where(h => h.Anio == AnioTarget && promosListIds.Contains(h.EstudianteId))
            .ExecuteUpdateAsync(s => s
                .SetProperty(h => h.EstadoFinal, "Promocional")
                .SetProperty(h => h.NotaFinal,   h => (decimal?)(8 + h.EstudianteId % 3))
                .SetProperty(h => h.Condicion,   CondicionEstudiante.Promocional), ct);

        logger.LogInformation("CondicionRealistaSeeder: HistorialAcademico actualizado.");

        // ── 5. InscripcionMateria 2024 (sólo Libre → Desaprobada) ─────────────
        var cursoIds2024 = await db.Cursos
            .Where(c => c.Anio == AnioTarget)
            .Select(c => c.Id)
            .ToListAsync(ct);

        var inscMat = await db.InscripcionesMateria
            .Where(im => cursoIds2024.Contains(im.CursoId) && libresListIds.Contains(im.EstudianteId))
            .ToListAsync(ct);

        foreach (var im in inscMat) im.MarcarDesaprobada();
        await db.SaveChangesAsync(ct);
        logger.LogInformation("CondicionRealistaSeeder: InscripcionMateria actualizada ({N}).", inscMat.Count);

        // ── 6. Asistencias 2024 (Libre: sólo primeras 9 de 18 como Presente) ──
        var asistencias = await db.Asistencias
            .Where(a => a.Fecha.Year == AnioTarget && libresListIds.Contains(a.EstudianteId))
            .OrderBy(a => a.EstudianteId).ThenBy(a => a.Fecha)
            .ToListAsync(ct);

        foreach (var grupo in asistencias.GroupBy(a => a.EstudianteId))
        {
            var lista = grupo.OrderBy(a => a.Fecha).ToList();
            for (int i = 9; i < lista.Count; i++)
            {
                if (lista[i].Estado == EstadoAsistencia.Presente)
                    lista[i].Rectificar(EstadoAsistencia.Ausente, null);
            }
        }
        await db.SaveChangesAsync(ct);
        logger.LogInformation("CondicionRealistaSeeder: Asistencias ajustadas.");

        logger.LogInformation(
            "CondicionRealistaSeeder: completado — {L} Libre · {P} Promocional actualizados coherentemente.",
            aLibre.Count, aPromo.Count);
    }

    // Misma lógica que GenerarNota en Anio2024ActividadesSeeder
    private static decimal GenerarNota(
        CondicionEstudiante c, int estudianteId, int examenId, TipoExamen tipo)
    {
        int seed = estudianteId * 31 + examenId;
        return (c, tipo) switch
        {
            (CondicionEstudiante.Promocional, _)              => 8 + seed % 3,
            (CondicionEstudiante.Libre, TipoExamen.Final)     => 2 + seed % 4,
            (CondicionEstudiante.Libre, _)                    => 1 + seed % 3,
            _                                                 => 3
        };
    }
}
