using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.ValueObjects;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Siembra todas las actividades académicas del año 2025.
///
/// Grupos activos en 2025:
///   Profesorado 2025 (nuevo, Año 1)    — creados por NuevosEstudiantes2025Seeder
///   Trayecto     2025 (nuevo, Año 1)   — creados por NuevosEstudiantes2025TrayectoSeeder
///   Profesorado  2024 (Año 2)          — continúan; 8 desertan durante este año
///   Profesorado  2023 (Año 3)          — continúan (excluye los 8 desertores de 2024 Año 2)
///   Profesorado  2022 (Año 4)          — completan el programa desde 2024 Año 3
///   Trayecto     2024 (Año 2, egresa)  — completan el programa; DesertoresY2 abandonan
///
/// Idempotente: se omite si ya existen EspaciosCurriculares para cursos 2025.
/// </summary>
public static class Anio2025ActividadesSeeder
{
    private const int Anio = 2025;

    // Índices (0-based) dentro de cada comisión de 30 que desertan en Prof 2024 Año 2.
    private static readonly int[] IndicesDesertorProf2024Anio2 = [4, 11, 19, 27];

    private static readonly DateTime[] FechasClase =
    [
        new(2025, 3, 4),  new(2025, 3, 18),
        new(2025, 4, 1),  new(2025, 4, 15),
        new(2025, 5, 6),  new(2025, 5, 20),
        new(2025, 6, 3),  new(2025, 6, 17),
        new(2025, 7, 1),  new(2025, 7, 15),
        new(2025, 8, 5),  new(2025, 8, 19),
        new(2025, 9, 2),  new(2025, 9, 16),
        new(2025, 9, 30), new(2025, 10, 14),
        new(2025, 11, 4), new(2025, 11, 18),
    ];

    private static readonly DateTime FechaParcial1 = new(2025, 5, 21);
    private static readonly DateTime FechaParcial2 = new(2025, 8, 20);
    private static readonly DateTime FechaParcial3 = new(2025, 10, 15);
    private static readonly DateTime FechaFinal    = new(2025, 12, 3);
    private static readonly DateTime FechaEncuesta = new(2025, 11, 18);

    private static readonly string[] TextosEncuesta =
    [
        "La dinámica de clase es muy buena.",
        "Quisiera más ejemplos prácticos.",
        "Excelente nivel académico.",
        "La disponibilidad del docente es mejorable.",
        "Muy conforme con la cursada en general.",
    ];

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var cursoIds = await db.Cursos
            .Where(c => c.Anio == Anio)
            .Select(c => c.Id)
            .ToListAsync(ct);

        if (cursoIds.Count == 0)
        {
            logger.LogWarning("Anio2025ActividadesSeeder: no hay cursos de 2025.");
            return;
        }

        var desertoresProf2024 = await SeedInscripcionesContinuantes(db, cursoIds, logger, ct);
        logger.LogInformation(
            "Anio2025ActividadesSeeder: InscripcionesContinuantes — OK ({D} desertores Prof2024 Año2 identificados).",
            desertoresProf2024.Count);

        if (await db.EspaciosCurriculares.AnyAsync(ec => cursoIds.Contains(ec.CursoId), ct))
        {
            logger.LogInformation("Anio2025ActividadesSeeder: ya existe, omitido.");
            return;
        }

        var cursos     = await db.Cursos.Where(c => c.Anio == Anio).ToListAsync(ct);
        var materias   = await db.Materias.ToListAsync(ct);
        var docenteIds = await db.Docentes.Select(d => d.Id).OrderBy(d => d).ToListAsync(ct);

        var cursoAnioMap = cursos.ToDictionary(c => c.Id, c => c.AnioLectivo);

        var inscripciones = await db.InscripcionesMateria
            .Include(im => im.Estudiante)
            .Where(im => cursoIds.Contains(im.CursoId))
            .ToListAsync(ct);

        var materias2025 = materias
            .Where(m => cursos.Any(c => c.CarreraId == m.CarreraId && c.AnioLectivo == m.Anio))
            .ToList();

        await SeedEspaciosCurriculares(db, cursos, materias, docenteIds, ct);
        logger.LogInformation("Anio2025ActividadesSeeder: EspaciosCurriculares — OK.");

        await SeedAsistencias(db, inscripciones, cursoAnioMap, desertoresProf2024, ct);
        logger.LogInformation("Anio2025ActividadesSeeder: Asistencias — OK.");

        var examenesPorMateria = await SeedExamenes(db, materias2025, ct);
        logger.LogInformation("Anio2025ActividadesSeeder: Exámenes — OK.");

        await SeedInscripcionesExamen(db, inscripciones, examenesPorMateria, cursoAnioMap, desertoresProf2024, ct);
        logger.LogInformation("Anio2025ActividadesSeeder: InscripcionesExamen — OK.");

        await SeedHistorialAcademico(db, inscripciones, cursos, cursoAnioMap, desertoresProf2024, ct);
        logger.LogInformation("Anio2025ActividadesSeeder: HistorialAcademico — OK.");

        await SeedEncuestas(db, materias2025, inscripciones, cursoAnioMap, desertoresProf2024, ct);
        logger.LogInformation("Anio2025ActividadesSeeder: Encuestas — OK.");

        await UpdateEstados(db, inscripciones, cursoAnioMap, desertoresProf2024, ct);
        logger.LogInformation("Anio2025ActividadesSeeder: Estados — OK.");

        await FixLibreConFinalAprobado(db, cursoIds, ct);
        logger.LogInformation("Anio2025ActividadesSeeder: FixLibreAprobado — OK.");

        await MarcarDesertoresProf2024Anio2(db, desertoresProf2024, ct);
        logger.LogInformation("Anio2025ActividadesSeeder: DesertoresProf2024 marcados — OK.");

        await UpdateAnioEstudiantes(db, cursoIds, ct);
        logger.LogInformation("Anio2025ActividadesSeeder: Anio estudiantes — OK.");

        logger.LogInformation(
            "Anio2025ActividadesSeeder: completado — {N} inscripciones procesadas.", inscripciones.Count);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    private static CondicionEstudiante CondicionEfectiva(
        CondicionEstudiante condicion, int anioEstudiante, int anioLectivoCurso)
    {
        if (condicion == CondicionEstudiante.Desertor && anioEstudiante > anioLectivoCurso)
            return CondicionEstudiante.Regular;
        return condicion;
    }

    // ──────────────────────────────────────────────────────────────────────────────
    private static async Task<HashSet<int>> SeedInscripcionesContinuantes(
        AppDbContext db, List<int> cursoIds2025, ILogger logger, CancellationToken ct)
    {
        var yaExistentes = (await db.InscripcionesMateria
            .Where(im => cursoIds2025.Contains(im.CursoId))
            .Select(im => new { im.EstudianteId, im.MateriaId, im.CursoId })
            .ToListAsync(ct))
            .Select(x => (x.EstudianteId, x.MateriaId, x.CursoId))
            .ToHashSet();

        var cursos2025Map = (await db.Cursos.Where(c => c.Anio == Anio).ToListAsync(ct))
            .ToDictionary(c => (c.CarreraId, c.AnioLectivo, c.Comision), c => c.Id);

        var materiaMap = (await db.Materias.ToListAsync(ct))
            .GroupBy(m => (m.CarreraId, m.Anio))
            .ToDictionary(g => g.Key, g => g.Select(m => m.Id).ToList());

        var nuevas = new List<InscripcionMateria>();
        var desertoresProf2024 = new HashSet<int>();

        // ── a) Trayecto 2025 → Año 1 ────────────────────────────────────────────
        var tray2025 = await db.Estudiantes
            .Include(e => e.Usuario)
            .Where(e => e.FechaDeIngreso.Year == 2025 && e.CarreraId == 2)
            .ToListAsync(ct);

        if (materiaMap.TryGetValue((2, 1), out var matTray1))
        {
            foreach (var est in tray2025)
            {
                if (!TryParsarComision(est.Usuario.Legajo, out var com)) continue;
                if (!cursos2025Map.TryGetValue((2, 1, com), out var cursoId)) continue;
                foreach (var matId in matTray1)
                    if (yaExistentes.Add((est.Id, matId, cursoId)))
                        nuevas.Add(InscripcionMateria.Crear(est.Id, matId, cursoId));
            }
        }

        // ── b) Profesorado 2022 → Año 4 ─────────────────────────────────────────
        var cursosProf2024Y3 = await db.Cursos
            .Where(c => c.Anio == 2024 && c.AnioLectivo == 3 && c.CarreraId == 1)
            .ToListAsync(ct);
        var cursosProf2024Y3Map = cursosProf2024Y3.ToDictionary(c => c.Id, c => c.Comision);

        var estIds2022 = await db.Estudiantes
            .Where(e => e.FechaDeIngreso.Year == 2022 && e.CarreraId == 1)
            .Select(e => e.Id)
            .ToListAsync(ct);

        var prof2022Cont = (await db.InscripcionesMateria
            .Where(im => cursosProf2024Y3Map.Keys.Contains(im.CursoId)
                && im.Estado != EstadoInscripcion.Baja
                && estIds2022.Contains(im.EstudianteId))
            .Select(im => new { im.EstudianteId, im.CursoId })
            .ToListAsync(ct))
            .GroupBy(x => x.EstudianteId)
            .Select(g => (EstId: g.Key, Comision: cursosProf2024Y3Map[g.First().CursoId]))
            .ToList();

        if (materiaMap.TryGetValue((1, 4), out var matProf4))
        {
            foreach (var (estId, com) in prof2022Cont)
            {
                if (!cursos2025Map.TryGetValue((1, 4, com), out var cursoId)) continue;
                foreach (var matId in matProf4)
                    if (yaExistentes.Add((estId, matId, cursoId)))
                        nuevas.Add(InscripcionMateria.Crear(estId, matId, cursoId));
            }
        }

        // ── c) Profesorado 2023 → Año 3 ─────────────────────────────────────────
        var cursosProf2024Y2 = await db.Cursos
            .Where(c => c.Anio == 2024 && c.AnioLectivo == 2 && c.CarreraId == 1)
            .ToListAsync(ct);
        var cursosProf2024Y2Map = cursosProf2024Y2.ToDictionary(c => c.Id, c => c.Comision);

        var estIds2023 = await db.Estudiantes
            .Where(e => e.FechaDeIngreso.Year == 2023 && e.CarreraId == 1)
            .Select(e => e.Id)
            .ToListAsync(ct);

        var prof2023Cont = (await db.InscripcionesMateria
            .Where(im => cursosProf2024Y2Map.Keys.Contains(im.CursoId)
                && im.Estado != EstadoInscripcion.Baja
                && estIds2023.Contains(im.EstudianteId))
            .Select(im => new { im.EstudianteId, im.CursoId })
            .ToListAsync(ct))
            .GroupBy(x => x.EstudianteId)
            .Select(g => (EstId: g.Key, Comision: cursosProf2024Y2Map[g.First().CursoId]))
            .ToList();

        if (materiaMap.TryGetValue((1, 3), out var matProf3))
        {
            foreach (var (estId, com) in prof2023Cont)
            {
                if (!cursos2025Map.TryGetValue((1, 3, com), out var cursoId)) continue;
                foreach (var matId in matProf3)
                    if (yaExistentes.Add((estId, matId, cursoId)))
                        nuevas.Add(InscripcionMateria.Crear(estId, matId, cursoId));
            }
        }

        // ── d) Profesorado 2024 → Año 2 (con 8 desertores distribuidos) ─────────
        if (materiaMap.TryGetValue((1, 2), out var matProf2))
        {
            var prof2024Todos = await db.Estudiantes
                .Include(e => e.Usuario)
                .Where(e => e.FechaDeIngreso.Year == 2024
                         && e.CarreraId == 1
                         && e.Condicion == CondicionEstudiante.Regular)
                .ToListAsync(ct);

            foreach (var comision in new[] { "A", "B" })
            {
                if (!cursos2025Map.TryGetValue((1, 2, comision), out var cursoId)) continue;

                var estComision = prof2024Todos
                    .Where(e => e.Usuario.Legajo.Contains($"C1{comision}"))
                    .OrderBy(e => e.Usuario.Legajo)
                    .ToList();

                for (int i = 0; i < estComision.Count; i++)
                {
                    var est = estComision[i];
                    foreach (var matId in matProf2)
                        if (yaExistentes.Add((est.Id, matId, cursoId)))
                            nuevas.Add(InscripcionMateria.Crear(est.Id, matId, cursoId));

                    if (IndicesDesertorProf2024Anio2.Contains(i))
                        desertoresProf2024.Add(est.Id);
                }
            }
        }

        // ── e) Trayecto 2024 → Año 2 (excluye DesertoresY1) ────────────────────
        var tray2024Cont = await db.Estudiantes
            .Include(e => e.Usuario)
            .Where(e => e.FechaDeIngreso.Year == 2024
                     && e.CarreraId == 2
                     && !(e.Condicion == CondicionEstudiante.Desertor && e.Anio == 1))
            .ToListAsync(ct);

        if (materiaMap.TryGetValue((2, 2), out var matTray2))
        {
            foreach (var est in tray2024Cont)
            {
                if (!TryParsarComision(est.Usuario.Legajo, out var com)) continue;
                if (!cursos2025Map.TryGetValue((2, 2, com), out var cursoId)) continue;
                foreach (var matId in matTray2)
                    if (yaExistentes.Add((est.Id, matId, cursoId)))
                        nuevas.Add(InscripcionMateria.Crear(est.Id, matId, cursoId));
            }
        }

        if (nuevas.Count > 0)
        {
            db.InscripcionesMateria.AddRange(nuevas);
            await db.SaveChangesAsync(ct);

            var nuevosCursoIds = nuevas.Select(n => n.CursoId).Distinct().ToList();
            await db.InscripcionesMateria
                .Where(im => nuevosCursoIds.Contains(im.CursoId))
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.FechaInscripcion, new DateTime(2025, 3, 1)), ct);

            logger.LogInformation("Anio2025ActividadesSeeder: {N} inscripciones de continuantes creadas.", nuevas.Count);
        }

        return desertoresProf2024;
    }

    private static bool TryParsarComision(string legajo, out string comision)
    {
        comision = string.Empty;
        var partes = legajo.Split('-');
        if (partes.Length < 4 || !partes[2].StartsWith('C')) return false;
        var parte = partes[2][1..];
        if (parte.Length < 2) return false;
        comision = parte[^1..].ToUpperInvariant();
        return true;
    }

    // ──────────────────────────────────────────────────────────────────────────────
    private static async Task SeedEspaciosCurriculares(
        AppDbContext db, List<Curso> cursos, List<Materia> materias,
        List<int> docenteIds, CancellationToken ct)
    {
        var materias2025 = materias
            .Where(m => cursos.Any(c => c.CarreraId == m.CarreraId && c.AnioLectivo == m.Anio))
            .ToList();

        var map = new Dictionary<int, int>();
        int idx = 0;
        foreach (var m in materias2025)
            map[m.Id] = docenteIds[idx++ % docenteIds.Count];

        foreach (var curso in cursos)
        {
            foreach (var materia in materias.Where(m => m.CarreraId == curso.CarreraId && m.Anio == curso.AnioLectivo))
                db.EspaciosCurriculares.Add(EspacioCurricular.Crear(materia.Id, map[materia.Id], curso.Id));
        }

        await db.SaveChangesAsync(ct);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    private static async Task SeedAsistencias(
        AppDbContext db, List<InscripcionMateria> inscripciones,
        Dictionary<int, int> cursoAnioMap, HashSet<int> desertoresProf2024,
        CancellationToken ct)
    {
        int batch = 0;
        foreach (var insc in inscripciones)
        {
            int anioLectivo  = cursoAnioMap.GetValueOrDefault(insc.CursoId, 1);
            bool esDesertor  = desertoresProf2024.Contains(insc.EstudianteId);
            var condEfectiva = esDesertor
                ? CondicionEstudiante.Desertor
                : CondicionEfectiva(insc.Estudiante.Condicion, insc.Estudiante.Anio, anioLectivo);

            for (int i = 0; i < FechasClase.Length; i++)
                db.Asistencias.Add(Asistencia.Registrar(
                    insc.EstudianteId, insc.MateriaId, insc.CursoId,
                    FechasClase[i], CalcularAsistencia(condEfectiva, i)));

            if (++batch % 100 == 0) await db.SaveChangesAsync(ct);
        }
        await db.SaveChangesAsync(ct);
    }

    private static EstadoAsistencia CalcularAsistencia(CondicionEstudiante c, int idx) => c switch
    {
        CondicionEstudiante.Promocional => idx < 15 ? EstadoAsistencia.Presente : EstadoAsistencia.Ausente,
        CondicionEstudiante.Egresado    => idx < 15 ? EstadoAsistencia.Presente : EstadoAsistencia.Ausente,
        CondicionEstudiante.Regular     => idx % 3 != 2 ? EstadoAsistencia.Presente : EstadoAsistencia.Ausente,
        CondicionEstudiante.Libre       => idx < 9 ? EstadoAsistencia.Presente : EstadoAsistencia.Ausente,
        CondicionEstudiante.Desertor    => idx < 3 ? EstadoAsistencia.Presente : EstadoAsistencia.Ausente,
        _                               => EstadoAsistencia.Presente
    };

    // ──────────────────────────────────────────────────────────────────────────────
    private static async Task<Dictionary<int, List<Examen>>> SeedExamenes(
        AppDbContext db, List<Materia> materias, CancellationToken ct)
    {
        var result = new Dictionary<int, List<Examen>>();
        foreach (var materia in materias)
        {
            result[materia.Id] = [];
            foreach (var fecha in (DateTime[])[FechaParcial1, FechaParcial2, FechaParcial3])
            {
                var parcial = Examen.CrearHistorico(materia.Id, fecha, "08:00", 60, TipoExamen.Parcial);
                db.Examenes.Add(parcial);
                result[materia.Id].Add(parcial);
            }
            var final = Examen.CrearHistorico(materia.Id, FechaFinal, "08:00", 60, TipoExamen.Final);
            db.Examenes.Add(final);
            result[materia.Id].Add(final);
        }
        await db.SaveChangesAsync(ct);
        return result;
    }

    // ──────────────────────────────────────────────────────────────────────────────
    private static async Task SeedInscripcionesExamen(
        AppDbContext db, List<InscripcionMateria> inscripciones,
        Dictionary<int, List<Examen>> examenesPorMateria,
        Dictionary<int, int> cursoAnioMap, HashSet<int> desertoresProf2024,
        CancellationToken ct)
    {
        foreach (var insc in inscripciones)
        {
            if (desertoresProf2024.Contains(insc.EstudianteId)) continue;

            int anioLectivo  = cursoAnioMap.GetValueOrDefault(insc.CursoId, 1);
            var condEfectiva = CondicionEfectiva(
                insc.Estudiante.Condicion, insc.Estudiante.Anio, anioLectivo);

            if (condEfectiva == CondicionEstudiante.Desertor) continue;
            if (!examenesPorMateria.TryGetValue(insc.MateriaId, out var examenes)) continue;

            foreach (var examen in examenes)
            {
                if (examen.TipoExamen == TipoExamen.Final && condEfectiva == CondicionEstudiante.Promocional)
                    continue;

                var ie = InscripcionExamen.Crear(insc.EstudianteId, examen.Id);
                ie.CargarNota(Nota.Crear(
                    GenerarNota(condEfectiva, insc.EstudianteId, examen.Id, examen.TipoExamen)));
                db.InscripcionesExamen.Add(ie);
            }
        }
        await db.SaveChangesAsync(ct);
    }

    private static decimal GenerarNota(
        CondicionEstudiante c, int estudianteId, int examenId,
        TipoExamen tipo = TipoExamen.Parcial)
    {
        int seed = estudianteId * 31 + examenId;
        return (c, tipo) switch
        {
            (CondicionEstudiante.Promocional, _)                 => 8 + seed % 3,
            (CondicionEstudiante.Egresado,    _)                 => 8 + seed % 3,
            (CondicionEstudiante.Regular,     TipoExamen.Final)  => 5 + seed % 4,
            (CondicionEstudiante.Regular,     _)                 => 3 + seed % 5,
            (CondicionEstudiante.Libre,       TipoExamen.Final)  => 2 + seed % 4,
            (CondicionEstudiante.Libre,       _)                 => 1 + seed % 3,
            _                                                    => 3
        };
    }

    // ──────────────────────────────────────────────────────────────────────────────
    private static async Task SeedHistorialAcademico(
        AppDbContext db, List<InscripcionMateria> inscripciones,
        List<Curso> cursos, Dictionary<int, int> cursoAnioMap,
        HashSet<int> desertoresProf2024, CancellationToken ct)
    {
        var cursoPorId = cursos.ToDictionary(c => c.Id);
        foreach (var insc in inscripciones)
        {
            if (!cursoPorId.TryGetValue(insc.CursoId, out var curso)) continue;

            int anioLectivo  = cursoAnioMap.GetValueOrDefault(insc.CursoId, 1);
            bool esDesertor  = desertoresProf2024.Contains(insc.EstudianteId);
            var condEfectiva = esDesertor
                ? CondicionEstudiante.Desertor
                : CondicionEfectiva(insc.Estudiante.Condicion, insc.Estudiante.Anio, anioLectivo);

            var (estado, nota) = EstadoFinal(condEfectiva, insc.EstudianteId);
            db.HistorialAcademico.Add(HistorialAcademico.Crear(
                insc.EstudianteId, insc.MateriaId, insc.CursoId,
                Anio, curso.Comision, estado, nota, condEfectiva));
        }
        await db.SaveChangesAsync(ct);
    }

    private static (string estado, decimal? nota) EstadoFinal(CondicionEstudiante c, int id) => c switch
    {
        CondicionEstudiante.Promocional => ("Promocional",  (decimal)(8 + id % 3)),
        CondicionEstudiante.Egresado    => ("Egresado",     (decimal)(8 + id % 3)),
        CondicionEstudiante.Regular     => ("Regularizado", (decimal)(4 + id % 4)),
        CondicionEstudiante.Libre       => ("Libre",        (decimal)(1 + id % 3)),
        CondicionEstudiante.Desertor    => ("Abandonó",     null),
        _                               => ("Regular",      null)
    };

    // ──────────────────────────────────────────────────────────────────────────────
    private static async Task SeedEncuestas(
        AppDbContext db, List<Materia> materias,
        List<InscripcionMateria> inscripciones,
        Dictionary<int, int> cursoAnioMap, HashSet<int> desertoresProf2024,
        CancellationToken ct)
    {
        foreach (var materia in materias)
        {
            var encuesta = Encuesta.Crear(
                $"Evaluación Docente 2025 — {materia.Nombre}",
                TipoEncuesta.EvaluacionDocente, Anio, materiaId: materia.Id);
            encuesta.Desactivar();
            db.Encuestas.Add(encuesta);
            await db.SaveChangesAsync(ct);

            var p1 = PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo evaluás la claridad explicativa del docente?", 1, TipoPregunta.EscalaLikert);
            var p2 = PreguntaEncuesta.Crear(encuesta.Id, "¿Qué aspectos mejorarías de la cursada?", 2, TipoPregunta.TextoLibre, false);
            db.PreguntasEncuesta.AddRange(p1, p2);
            await db.SaveChangesAsync(ct);

            var respondentes = inscripciones
                .Where(im => im.MateriaId == materia.Id
                    && !desertoresProf2024.Contains(im.EstudianteId)
                    && CondicionEfectiva(
                           im.Estudiante.Condicion,
                           im.Estudiante.Anio,
                           cursoAnioMap.GetValueOrDefault(im.CursoId, 1))
                       != CondicionEstudiante.Desertor)
                .Select(im => im.EstudianteId)
                .Distinct()
                .ToList();

            int participan = (int)(respondentes.Count * 0.75);
            var respuestas = Enumerable.Range(0, participan)
                .Select(_ => RespuestaEncuesta.Crear(encuesta.Id, FechaEncuesta))
                .ToList();
            db.RespuestasEncuesta.AddRange(respuestas);
            await db.SaveChangesAsync(ct);

            var items = new List<ItemRespuesta>();
            for (int i = 0; i < participan; i++)
            {
                int likert = 3 + respondentes[i] % 3;
                items.Add(ItemRespuesta.Crear(respuestas[i].Id, p1.Id, likert, null));
                items.Add(ItemRespuesta.Crear(respuestas[i].Id, p2.Id, null, TextosEncuesta[i % TextosEncuesta.Length]));
            }
            db.ItemsRespuesta.AddRange(items);
            await db.SaveChangesAsync(ct);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────────
    private static async Task UpdateEstados(
        AppDbContext db, List<InscripcionMateria> inscripciones,
        Dictionary<int, int> cursoAnioMap, HashSet<int> desertoresProf2024,
        CancellationToken ct)
    {
        foreach (var insc in inscripciones)
        {
            int anioLectivo  = cursoAnioMap.GetValueOrDefault(insc.CursoId, 1);
            bool esDesertor  = desertoresProf2024.Contains(insc.EstudianteId);
            var condEfectiva = esDesertor
                ? CondicionEstudiante.Desertor
                : CondicionEfectiva(insc.Estudiante.Condicion, insc.Estudiante.Anio, anioLectivo);

            switch (condEfectiva)
            {
                case CondicionEstudiante.Promocional:
                case CondicionEstudiante.Regular:
                case CondicionEstudiante.Egresado:
                    insc.MarcarAprobada(); break;
                case CondicionEstudiante.Libre:
                    insc.MarcarDesaprobada(); break;
                case CondicionEstudiante.Desertor:
                    insc.DarDeBaja(); break;
            }
        }
        await db.SaveChangesAsync(ct);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    private static async Task FixLibreConFinalAprobado(
        AppDbContext db, List<int> cursoIds, CancellationToken ct)
    {
        var inscLibreConFinalAprobado = await (
            from im in db.InscripcionesMateria
            join e  in db.Estudiantes on im.EstudianteId equals e.Id
            join c  in db.Cursos      on im.CursoId      equals c.Id
            where e.Condicion == CondicionEstudiante.Libre
               && im.Estado   == EstadoInscripcion.Desaprobada
               && cursoIds.Contains(im.CursoId)
               && db.InscripcionesExamen.Any(ie =>
                    ie.EstudianteId == im.EstudianteId
                    && ie.NotaValor >= 4
                    && db.Examenes.Any(ex =>
                        ex.Id         == ie.ExamenId
                        && ex.MateriaId == im.MateriaId
                        && ex.TipoExamen == TipoExamen.Final))
            select im
        ).ToListAsync(ct);

        foreach (var im in inscLibreConFinalAprobado)
            im.MarcarAprobada();

        if (inscLibreConFinalAprobado.Count > 0)
            await db.SaveChangesAsync(ct);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    private static async Task MarcarDesertoresProf2024Anio2(
        AppDbContext db, HashSet<int> desertoresProf2024, CancellationToken ct)
    {
        if (desertoresProf2024.Count == 0) return;

        await db.Estudiantes
            .Where(e => desertoresProf2024.Contains(e.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.Condicion, CondicionEstudiante.Desertor)
                .SetProperty(e => e.Anio, 2), ct);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    private static async Task UpdateAnioEstudiantes(
        AppDbContext db, List<int> cursoIds2025, CancellationToken ct)
    {
        var cursos2025 = await db.Cursos
            .Where(c => c.Anio == Anio)
            .ToListAsync(ct);

        foreach (var nivelGroup in cursos2025.GroupBy(c => c.AnioLectivo))
        {
            int nivel = nivelGroup.Key;
            var cursoIdsNivel = nivelGroup.Select(c => c.Id).ToList();

            var enNivel = await db.InscripcionesMateria
                .Where(im => cursoIdsNivel.Contains(im.CursoId))
                .Select(im => im.EstudianteId)
                .Distinct()
                .ToListAsync(ct);

            if (enNivel.Count > 0)
                await db.Estudiantes
                    .Where(e => enNivel.Contains(e.Id)
                             && e.Condicion != CondicionEstudiante.Desertor
                             && e.Anio < nivel)
                    .ExecuteUpdateAsync(s => s.SetProperty(e => e.Anio, nivel), ct);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Completa las actividades de Año 1 Trayecto 2025 para los estudiantes creados
    /// por NuevosEstudiantes2025TrayectoSeeder (que corre DESPUÉS del seeder principal).
    /// Guard propio: si ya existen Asistencias para estos estudiantes en 2025, se omite.
    /// </summary>
    public static async Task SeedTrayecto2025Año1CompletarAsync(
        AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var cursosTray2025Y1Ids = await db.Cursos
            .Where(c => c.Anio == 2025 && c.AnioLectivo == 1 && c.CarreraId == 2)
            .Select(c => c.Id)
            .ToListAsync(ct);

        if (cursosTray2025Y1Ids.Count == 0)
        {
            logger.LogWarning("SeedTrayecto2025Año1: no hay cursos Trayecto 2025 Año 1.");
            return;
        }

        var estIds = await db.Estudiantes
            .Where(e => e.FechaDeIngreso.Year == 2025 && e.CarreraId == 2)
            .Select(e => e.Id)
            .ToListAsync(ct);

        if (estIds.Count == 0)
        {
            logger.LogWarning("SeedTrayecto2025Año1: no hay estudiantes Trayecto 2025. Ejecutá NuevosEstudiantes2025TrayectoSeeder primero.");
            return;
        }

        bool yaExiste = await db.Asistencias
            .AnyAsync(a => estIds.Contains(a.EstudianteId) && a.Fecha.Year == 2025, ct);

        if (yaExiste)
        {
            logger.LogInformation("SeedTrayecto2025Año1: actividades ya existen, omitido.");
            return;
        }

        var inscripciones = await db.InscripcionesMateria
            .Include(im => im.Estudiante)
            .Where(im => cursosTray2025Y1Ids.Contains(im.CursoId) && estIds.Contains(im.EstudianteId))
            .ToListAsync(ct);

        if (inscripciones.Count == 0)
        {
            logger.LogWarning("SeedTrayecto2025Año1: sin inscripciones para Trayecto 2025 Año 1.");
            return;
        }

        // Asistencias
        int batch = 0;
        foreach (var insc in inscripciones)
        {
            var condEfectiva = CondicionEfectiva(insc.Estudiante.Condicion, insc.Estudiante.Anio, 1);
            for (int i = 0; i < FechasClase.Length; i++)
                db.Asistencias.Add(Asistencia.Registrar(
                    insc.EstudianteId, insc.MateriaId, insc.CursoId,
                    FechasClase[i], CalcularAsistencia(condEfectiva, i)));
            if (++batch % 100 == 0) await db.SaveChangesAsync(ct);
        }
        await db.SaveChangesAsync(ct);
        logger.LogInformation("SeedTrayecto2025Año1: Asistencias — OK.");

        // InscripcionesExamen
        var trayMateriaIds = inscripciones.Select(im => im.MateriaId).Distinct().ToList();
        var examenesTray = await db.Examenes
            .Where(e => e.FechaExamen.Year == 2025 && trayMateriaIds.Contains(e.MateriaId))
            .ToListAsync(ct);
        var examenesPorMateria = examenesTray
            .GroupBy(e => e.MateriaId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var insc in inscripciones)
        {
            var condEfectiva = CondicionEfectiva(insc.Estudiante.Condicion, insc.Estudiante.Anio, 1);
            if (condEfectiva == CondicionEstudiante.Desertor) continue;
            if (!examenesPorMateria.TryGetValue(insc.MateriaId, out var examenes)) continue;
            foreach (var examen in examenes)
            {
                if (examen.TipoExamen == TipoExamen.Final && condEfectiva == CondicionEstudiante.Promocional)
                    continue;
                var ie = InscripcionExamen.Crear(insc.EstudianteId, examen.Id);
                ie.CargarNota(Nota.Crear(GenerarNota(condEfectiva, insc.EstudianteId, examen.Id, examen.TipoExamen)));
                db.InscripcionesExamen.Add(ie);
            }
        }
        await db.SaveChangesAsync(ct);
        logger.LogInformation("SeedTrayecto2025Año1: InscripcionesExamen — OK.");

        // HistorialAcademico
        var cursoPorId = (await db.Cursos.Where(c => cursosTray2025Y1Ids.Contains(c.Id)).ToListAsync(ct))
            .ToDictionary(c => c.Id);
        foreach (var insc in inscripciones)
        {
            if (!cursoPorId.TryGetValue(insc.CursoId, out var curso)) continue;
            var condEfectiva = CondicionEfectiva(insc.Estudiante.Condicion, insc.Estudiante.Anio, 1);
            var (estado, nota) = EstadoFinal(condEfectiva, insc.EstudianteId);
            db.HistorialAcademico.Add(HistorialAcademico.Crear(
                insc.EstudianteId, insc.MateriaId, insc.CursoId,
                Anio, curso.Comision, estado, nota, condEfectiva));
        }
        await db.SaveChangesAsync(ct);
        logger.LogInformation("SeedTrayecto2025Año1: HistorialAcademico — OK.");

        // UpdateEstados
        foreach (var insc in inscripciones)
        {
            var condEfectiva = CondicionEfectiva(insc.Estudiante.Condicion, insc.Estudiante.Anio, 1);
            switch (condEfectiva)
            {
                case CondicionEstudiante.Promocional:
                case CondicionEstudiante.Regular:
                case CondicionEstudiante.Egresado:
                    insc.MarcarAprobada(); break;
                case CondicionEstudiante.Libre:
                    insc.MarcarDesaprobada(); break;
                case CondicionEstudiante.Desertor:
                    insc.DarDeBaja(); break;
            }
        }
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "SeedTrayecto2025Año1: completado — {N} inscripciones procesadas.", inscripciones.Count);
    }
}
