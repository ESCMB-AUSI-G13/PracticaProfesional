using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.ValueObjects;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Siembra todas las actividades académicas del año 2023.
///
/// Grupos activos en 2023:
///   Trayecto  2023 (nuevo, Año 1)  — ya existen en BD por CohorteHistoricaSeeder
///   Profesorado 2023 (nuevo, Año 1) — creados por NuevosEstudiantes2023Seeder
///   Profesorado 2021 (Año 3)        — continúan desde 2022 Año 2
///   Profesorado 2022 (Año 2)        — continúan desde 2022 Año 1
///   Trayecto  2022 (Año 2, egresa)  — continúan desde 2022 Año 1
///
/// Idempotente: se omite si ya existen EspaciosCurriculares para cursos 2023.
/// </summary>
public static class Anio2023ActividadesSeeder
{
    private const int Anio = 2023;

    private static readonly DateTime[] FechasClase =
    [
        new(2023,3,7),  new(2023,3,21),
        new(2023,4,4),  new(2023,4,18),
        new(2023,5,2),  new(2023,5,16),
        new(2023,6,6),  new(2023,6,20),
        new(2023,7,4),  new(2023,7,18),
        new(2023,8,1),  new(2023,8,15),
        new(2023,9,5),  new(2023,9,19),
        new(2023,10,3), new(2023,10,17),
        new(2023,11,7), new(2023,11,21),
    ];

    private static readonly DateTime FechaParcial1 = new(2023, 5, 17);
    private static readonly DateTime FechaParcial2 = new(2023, 8, 16);
    private static readonly DateTime FechaParcial3 = new(2023, 10, 18);
    private static readonly DateTime FechaFinal    = new(2023, 12, 6);
    private static readonly DateTime FechaEncuesta = new(2023, 11, 21);

    private static readonly string[] TextosEncuesta =
    [
        "Muy buena dinámica en clase, se explica con claridad.",
        "Me gustaría más tiempo para los trabajos prácticos.",
        "Excelente nivel de la cátedra.",
        "Podría mejorar la disponibilidad fuera del horario de clase.",
        "En general estoy muy satisfecho con la cursada.",
    ];

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var cursoIds = await db.Cursos
            .Where(c => c.Anio == Anio)
            .Select(c => c.Id)
            .ToListAsync(ct);

        if (cursoIds.Count == 0)
        {
            logger.LogWarning("Anio2023ActividadesSeeder: no hay cursos de 2023.");
            return;
        }

        if (await db.EspaciosCurriculares.AnyAsync(ec => cursoIds.Contains(ec.CursoId), ct))
        {
            logger.LogInformation("Anio2023ActividadesSeeder: ya existe, omitido.");
            return;
        }

        // Paso 1: inscripciones para grupos continuantes y Trayecto 2023
        await SeedInscripcionesContinuantes(db, cursoIds, logger, ct);
        logger.LogInformation("Anio2023ActividadesSeeder: InscripcionesContinuantes — OK.");

        var cursos   = await db.Cursos.Where(c => c.Anio == Anio).ToListAsync(ct);
        var materias = await db.Materias.ToListAsync(ct);
        var docenteIds = await db.Docentes.Select(d => d.Id).OrderBy(d => d).ToListAsync(ct);

        var inscripciones = await db.InscripcionesMateria
            .Include(im => im.Estudiante)
            .Where(im => cursoIds.Contains(im.CursoId))
            .ToListAsync(ct);

        var materias2023 = materias
            .Where(m => cursos.Any(c => c.CarreraId == m.CarreraId && c.AnioLectivo == m.Anio))
            .ToList();

        var materiaDocenteMap = await SeedEspaciosCurriculares(db, cursos, materias, docenteIds, ct);
        logger.LogInformation("Anio2023ActividadesSeeder: EspaciosCurriculares — OK.");

        await SeedAsistencias(db, inscripciones, ct);
        logger.LogInformation("Anio2023ActividadesSeeder: Asistencias — OK.");

        var examenesPorMateria = await SeedExamenes(db, materias2023, ct);
        logger.LogInformation("Anio2023ActividadesSeeder: Exámenes — OK.");

        await SeedInscripcionesExamen(db, inscripciones, examenesPorMateria, ct);
        logger.LogInformation("Anio2023ActividadesSeeder: InscripcionesExamen — OK.");

        await SeedHistorialAcademico(db, inscripciones, cursos, ct);
        logger.LogInformation("Anio2023ActividadesSeeder: HistorialAcademico — OK.");

        await SeedEncuestas(db, materias2023, inscripciones, ct);
        logger.LogInformation("Anio2023ActividadesSeeder: Encuestas — OK.");

        await UpdateEstados(db, inscripciones, ct);
        logger.LogInformation("Anio2023ActividadesSeeder: Estados — OK.");

        await UpdateAnioEstudiantes(db, cursoIds, ct);
        logger.LogInformation("Anio2023ActividadesSeeder: Anio estudiantes — OK.");

        logger.LogInformation(
            "Anio2023ActividadesSeeder: completado — {N} inscripciones procesadas.", inscripciones.Count);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Completa las actividades de Año 1 Trayecto 2023 para los estudiantes creados
    // por NuevosEstudiantes2023TrayectoSeeder (que corrió DESPUÉS del seeder principal).
    // Guard propio: si ya existen Asistencias para esos estudiantes, se omite.
    public static async Task SeedTrayecto2023Año1CompletarAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var cursosTray2023Y1Ids = await db.Cursos
            .Where(c => c.Anio == 2023 && c.AnioLectivo == 1 && c.CarreraId == 2)
            .Select(c => c.Id)
            .ToListAsync(ct);

        if (cursosTray2023Y1Ids.Count == 0)
        {
            logger.LogWarning("SeedTrayecto2023Año1: no hay cursos Trayecto 2023 Año 1.");
            return;
        }

        var estIds = await db.Estudiantes
            .Where(e => e.FechaDeIngreso.Year == 2023 && e.CarreraId == 2)
            .Select(e => e.Id)
            .ToListAsync(ct);

        if (estIds.Count == 0)
        {
            logger.LogWarning("SeedTrayecto2023Año1: no hay estudiantes Trayecto 2023. Ejecutá NuevosEstudiantes2023TrayectoSeeder primero.");
            return;
        }

        bool yaExiste = await db.Asistencias
            .AnyAsync(a => estIds.Contains(a.EstudianteId) && a.Fecha.Year == 2023, ct);

        if (yaExiste)
        {
            logger.LogInformation("SeedTrayecto2023Año1: actividades ya existen, omitido.");
            return;
        }

        var inscripciones = await db.InscripcionesMateria
            .Include(im => im.Estudiante)
            .Where(im => cursosTray2023Y1Ids.Contains(im.CursoId) && estIds.Contains(im.EstudianteId))
            .ToListAsync(ct);

        if (inscripciones.Count == 0)
        {
            logger.LogWarning("SeedTrayecto2023Año1: sin inscripciones para Trayecto 2023 Año 1.");
            return;
        }

        // Asistencias
        int batch = 0;
        foreach (var insc in inscripciones)
        {
            for (int i = 0; i < FechasClase.Length; i++)
                db.Asistencias.Add(Asistencia.Registrar(
                    insc.EstudianteId, insc.MateriaId, insc.CursoId,
                    FechasClase[i], CalcularAsistencia(insc.Estudiante.Condicion, i)));
            if (++batch % 100 == 0) await db.SaveChangesAsync(ct);
        }
        await db.SaveChangesAsync(ct);
        logger.LogInformation("SeedTrayecto2023Año1: Asistencias — OK.");

        // InscripcionesExamen (exámenes ya existen del seeder principal)
        var trayMateriaIds = inscripciones.Select(im => im.MateriaId).Distinct().ToList();
        var examenesTray = await db.Examenes
            .Where(e => e.FechaExamen.Year == 2023 && trayMateriaIds.Contains(e.MateriaId))
            .ToListAsync(ct);

        var examenesPorMateria = examenesTray
            .GroupBy(e => e.MateriaId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var insc in inscripciones)
        {
            if (insc.Estudiante.Condicion == CondicionEstudiante.Desertor) continue;
            if (!examenesPorMateria.TryGetValue(insc.MateriaId, out var examenes)) continue;
            foreach (var examen in examenes)
            {
                if (examen.TipoExamen == TipoExamen.Final && insc.Estudiante.Condicion == CondicionEstudiante.Promocional)
                    continue;
                var ie = InscripcionExamen.Crear(insc.EstudianteId, examen.Id);
                ie.CargarNota(Nota.Crear(GenerarNota(insc.Estudiante.Condicion, insc.EstudianteId, examen.Id, examen.TipoExamen)));
                db.InscripcionesExamen.Add(ie);
            }
        }
        await db.SaveChangesAsync(ct);
        logger.LogInformation("SeedTrayecto2023Año1: InscripcionesExamen — OK.");

        // HistorialAcademico
        var cursoPorId = (await db.Cursos.Where(c => cursosTray2023Y1Ids.Contains(c.Id)).ToListAsync(ct))
            .ToDictionary(c => c.Id);
        foreach (var insc in inscripciones)
        {
            if (!cursoPorId.TryGetValue(insc.CursoId, out var curso)) continue;
            var (estado, nota) = EstadoFinal(insc.Estudiante.Condicion, insc.EstudianteId);
            db.HistorialAcademico.Add(HistorialAcademico.Crear(
                insc.EstudianteId, insc.MateriaId, insc.CursoId,
                Anio, curso.Comision, estado, nota, insc.Estudiante.Condicion));
        }
        await db.SaveChangesAsync(ct);
        logger.LogInformation("SeedTrayecto2023Año1: HistorialAcademico — OK.");

        // UpdateEstados
        foreach (var insc in inscripciones)
        {
            switch (insc.Estudiante.Condicion)
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
        logger.LogInformation("SeedTrayecto2023Año1: completado — {N} inscripciones procesadas.", inscripciones.Count);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Crea inscripciones para todos los grupos que aún no tienen inscripciones en 2023:
    //   a) Trayecto 2023 (existen en BD, sin inscripciones) → Año 1 Trayecto 2023
    //   b) Profesorado 2021 con aprobado en 2022 Año 2    → Año 3 Profesorado 2023
    //   c) Profesorado 2022 no-desertores-Año1            → Año 2 Profesorado 2023
    //   d) Trayecto 2022 no-desertores-Año1               → Año 2 Trayecto 2023
    private static async Task SeedInscripcionesContinuantes(
        AppDbContext db, List<int> cursoIds2023, ILogger logger, CancellationToken ct)
    {
        var yaExistentes = (await db.InscripcionesMateria
            .Where(im => cursoIds2023.Contains(im.CursoId))
            .Select(im => new { im.EstudianteId, im.MateriaId, im.CursoId })
            .ToListAsync(ct))
            .Select(x => (x.EstudianteId, x.MateriaId, x.CursoId))
            .ToHashSet();

        var cursos2023Map = (await db.Cursos.Where(c => c.Anio == Anio).ToListAsync(ct))
            .ToDictionary(c => (c.CarreraId, c.AnioLectivo, c.Comision), c => c.Id);

        var materiaMap = (await db.Materias.ToListAsync(ct))
            .GroupBy(m => (m.CarreraId, m.Anio))
            .ToDictionary(g => g.Key, g => g.Select(m => m.Id).ToList());

        var nuevas = new List<InscripcionMateria>();

        // ── a) Trayecto 2023 → Año 1 ────────────────────────────────────────────
        var tray2023 = await db.Estudiantes
            .Include(e => e.Usuario)
            .Where(e => e.FechaDeIngreso.Year == 2023 && e.CarreraId == 2)
            .ToListAsync(ct);

        if (materiaMap.TryGetValue((2, 1), out var matTray1))
        {
            foreach (var est in tray2023)
            {
                if (!TryParsarComision(est.Usuario.Legajo, out var com)) continue;
                if (!cursos2023Map.TryGetValue((2, 1, com), out var cursoId)) continue;
                foreach (var matId in matTray1)
                    if (yaExistentes.Add((est.Id, matId, cursoId)))
                        nuevas.Add(InscripcionMateria.Crear(est.Id, matId, cursoId));
            }
        }

        // ── b) Profesorado 2021 → Año 3 ─────────────────────────────────────────
        var cursosProf2022Y2 = await db.Cursos
            .Where(c => c.Anio == 2022 && c.AnioLectivo == 2 && c.CarreraId == 1)
            .ToListAsync(ct);
        var cursosProf2022Y2Map = cursosProf2022Y2.ToDictionary(c => c.Id, c => c.Comision);
        var cursosProf2022Y2Ids = cursosProf2022Y2Map.Keys.ToList();

        var estIds2021Prof = await db.Estudiantes
            .Where(e => e.FechaDeIngreso.Year == 2021 && e.CarreraId == 1)
            .Select(e => e.Id)
            .ToListAsync(ct);

        var prof2021Aprobados = (await db.InscripcionesMateria
            .Where(im => cursosProf2022Y2Ids.Contains(im.CursoId)
                && im.Estado != EstadoInscripcion.Baja
                && estIds2021Prof.Contains(im.EstudianteId))
            .Select(im => new { im.EstudianteId, im.CursoId })
            .ToListAsync(ct))
            .GroupBy(x => x.EstudianteId)
            .Select(g => (EstId: g.Key, Comision: cursosProf2022Y2Map[g.First().CursoId]))
            .ToList();

        if (materiaMap.TryGetValue((1, 3), out var matProf3))
        {
            foreach (var (estId, com) in prof2021Aprobados)
            {
                if (!cursos2023Map.TryGetValue((1, 3, com), out var cursoId)) continue;
                foreach (var matId in matProf3)
                    if (yaExistentes.Add((estId, matId, cursoId)))
                        nuevas.Add(InscripcionMateria.Crear(estId, matId, cursoId));
            }
        }

        // ── c) Profesorado 2022 → Año 2 ─────────────────────────────────────────
        var cursosProf2022Y1 = await db.Cursos
            .Where(c => c.Anio == 2022 && c.AnioLectivo == 1 && c.CarreraId == 1)
            .ToListAsync(ct);
        var cursosProf2022Y1Map = cursosProf2022Y1.ToDictionary(c => c.Id, c => c.Comision);
        var cursosProf2022Y1Ids = cursosProf2022Y1Map.Keys.ToList();

        var desertoresY1Prof2022 = await db.Estudiantes
            .Where(e => e.FechaDeIngreso.Year == 2022 && e.CarreraId == 1
                && e.Condicion == CondicionEstudiante.Desertor && e.Anio == 1)
            .Select(e => e.Id)
            .ToListAsync(ct);

        var prof2022Continuantes = (await db.InscripcionesMateria
            .Where(im => cursosProf2022Y1Ids.Contains(im.CursoId)
                && !desertoresY1Prof2022.Contains(im.EstudianteId))
            .Select(im => new { im.EstudianteId, im.CursoId })
            .ToListAsync(ct))
            .GroupBy(x => x.EstudianteId)
            .Select(g => (EstId: g.Key, Comision: cursosProf2022Y1Map[g.First().CursoId]))
            .ToList();

        if (materiaMap.TryGetValue((1, 2), out var matProf2))
        {
            foreach (var (estId, com) in prof2022Continuantes)
            {
                if (!cursos2023Map.TryGetValue((1, 2, com), out var cursoId)) continue;
                foreach (var matId in matProf2)
                    if (yaExistentes.Add((estId, matId, cursoId)))
                        nuevas.Add(InscripcionMateria.Crear(estId, matId, cursoId));
            }
        }

        // ── d) Trayecto 2022 → Año 2 ────────────────────────────────────────────
        var cursosTray2022Y1 = await db.Cursos
            .Where(c => c.Anio == 2022 && c.AnioLectivo == 1 && c.CarreraId == 2)
            .ToListAsync(ct);
        var cursosTray2022Y1Map = cursosTray2022Y1.ToDictionary(c => c.Id, c => c.Comision);
        var cursosTray2022Y1Ids = cursosTray2022Y1Map.Keys.ToList();

        var desertoresY1Tray2022 = await db.Estudiantes
            .Where(e => e.FechaDeIngreso.Year == 2022 && e.CarreraId == 2
                && e.Condicion == CondicionEstudiante.Desertor && e.Anio == 1)
            .Select(e => e.Id)
            .ToListAsync(ct);

        var tray2022Continuantes = (await db.InscripcionesMateria
            .Where(im => cursosTray2022Y1Ids.Contains(im.CursoId)
                && !desertoresY1Tray2022.Contains(im.EstudianteId))
            .Select(im => new { im.EstudianteId, im.CursoId })
            .ToListAsync(ct))
            .GroupBy(x => x.EstudianteId)
            .Select(g => (EstId: g.Key, Comision: cursosTray2022Y1Map[g.First().CursoId]))
            .ToList();

        if (materiaMap.TryGetValue((2, 2), out var matTray2))
        {
            foreach (var (estId, com) in tray2022Continuantes)
            {
                if (!cursos2023Map.TryGetValue((2, 2, com), out var cursoId)) continue;
                foreach (var matId in matTray2)
                    if (yaExistentes.Add((estId, matId, cursoId)))
                        nuevas.Add(InscripcionMateria.Crear(estId, matId, cursoId));
            }
        }

        if (nuevas.Count == 0)
        {
            logger.LogInformation("Anio2023ActividadesSeeder: sin inscripciones nuevas de continuantes.");
            return;
        }

        db.InscripcionesMateria.AddRange(nuevas);
        await db.SaveChangesAsync(ct);

        // Ajustar FechaInscripcion de los cursos recién poblados
        var nuevosCursoIds = nuevas.Select(n => n.CursoId).Distinct().ToList();
        await db.InscripcionesMateria
            .Where(im => nuevosCursoIds.Contains(im.CursoId))
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.FechaInscripcion, new DateTime(2023, 3, 1)), ct);

        logger.LogInformation(
            "Anio2023ActividadesSeeder: {N} inscripciones de continuantes creadas.", nuevas.Count);
    }

    private static bool TryParsarComision(string legajo, out string comision)
    {
        comision = string.Empty;
        var partes = legajo.Split('-');
        if (partes.Length < 4 || !partes[2].StartsWith('C')) return false;
        var parte = partes[2][1..]; // "2A" o "1B"
        if (parte.Length < 2) return false;
        comision = parte[^1..].ToUpperInvariant();
        return true;
    }

    // ──────────────────────────────────────────────────────────────────────────────
    private static async Task<Dictionary<int, int>> SeedEspaciosCurriculares(
        AppDbContext db, List<Curso> cursos, List<Materia> materias,
        List<int> docenteIds, CancellationToken ct)
    {
        var materias2023 = materias
            .Where(m => cursos.Any(c => c.CarreraId == m.CarreraId && c.AnioLectivo == m.Anio))
            .ToList();

        var map = new Dictionary<int, int>();
        int idx = 0;
        foreach (var m in materias2023)
            map[m.Id] = docenteIds[idx++ % docenteIds.Count];

        foreach (var curso in cursos)
        {
            foreach (var materia in materias.Where(m => m.CarreraId == curso.CarreraId && m.Anio == curso.AnioLectivo))
                db.EspaciosCurriculares.Add(EspacioCurricular.Crear(materia.Id, map[materia.Id], curso.Id));
        }

        await db.SaveChangesAsync(ct);
        return map;
    }

    // ──────────────────────────────────────────────────────────────────────────────
    private static async Task SeedAsistencias(
        AppDbContext db, List<InscripcionMateria> inscripciones, CancellationToken ct)
    {
        int batch = 0;
        foreach (var insc in inscripciones)
        {
            var condicion = insc.Estudiante.Condicion;
            for (int i = 0; i < FechasClase.Length; i++)
                db.Asistencias.Add(Asistencia.Registrar(
                    insc.EstudianteId, insc.MateriaId, insc.CursoId,
                    FechasClase[i], CalcularAsistencia(condicion, i)));

            if (++batch % 100 == 0)
                await db.SaveChangesAsync(ct);
        }
        await db.SaveChangesAsync(ct);
    }

    private static EstadoAsistencia CalcularAsistencia(CondicionEstudiante c, int idx) => c switch
    {
        CondicionEstudiante.Promocional => idx < 15 ? EstadoAsistencia.Presente : EstadoAsistencia.Ausente,
        CondicionEstudiante.Egresado    => idx < 15 ? EstadoAsistencia.Presente : EstadoAsistencia.Ausente,
        CondicionEstudiante.Regular     => idx % 3 != 2 ? EstadoAsistencia.Presente : EstadoAsistencia.Ausente,
        CondicionEstudiante.Libre       => idx < 9 ? EstadoAsistencia.Presente : EstadoAsistencia.Ausente,
        CondicionEstudiante.Desertor    => idx < 4 ? EstadoAsistencia.Presente : EstadoAsistencia.Ausente,
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
        Dictionary<int, List<Examen>> examenesPorMateria, CancellationToken ct)
    {
        foreach (var insc in inscripciones)
        {
            var condicion = insc.Estudiante.Condicion;
            if (condicion == CondicionEstudiante.Desertor) continue;
            if (!examenesPorMateria.TryGetValue(insc.MateriaId, out var examenes)) continue;

            foreach (var examen in examenes)
            {
                // Promocionales no rinden el Final
                if (examen.TipoExamen == TipoExamen.Final && condicion == CondicionEstudiante.Promocional)
                    continue;

                var ie = InscripcionExamen.Crear(insc.EstudianteId, examen.Id);
                ie.CargarNota(Nota.Crear(GenerarNota(condicion, insc.EstudianteId, examen.Id, examen.TipoExamen)));
                db.InscripcionesExamen.Add(ie);
            }
        }
        await db.SaveChangesAsync(ct);
    }

    private static decimal GenerarNota(CondicionEstudiante c, int estudianteId, int examenId, TipoExamen tipo = TipoExamen.Parcial)
    {
        int seed = estudianteId * 31 + examenId;
        return (c, tipo) switch
        {
            (CondicionEstudiante.Promocional, _)               => 8 + seed % 3,
            (CondicionEstudiante.Egresado,    _)               => 8 + seed % 3,
            (CondicionEstudiante.Regular,     TipoExamen.Final) => 6 + seed % 3,
            (CondicionEstudiante.Regular,     _)               => 5 + seed % 3,
            (CondicionEstudiante.Libre,       TipoExamen.Final) => 2 + seed % 5,
            (CondicionEstudiante.Libre,       _)               => 1 + seed % 4,
            _                                                  => 3
        };
    }

    // ──────────────────────────────────────────────────────────────────────────────
    private static async Task SeedHistorialAcademico(
        AppDbContext db, List<InscripcionMateria> inscripciones,
        List<Curso> cursos, CancellationToken ct)
    {
        var cursoPorId = cursos.ToDictionary(c => c.Id);
        foreach (var insc in inscripciones)
        {
            if (!cursoPorId.TryGetValue(insc.CursoId, out var curso)) continue;
            var (estado, nota) = EstadoFinal(insc.Estudiante.Condicion, insc.EstudianteId);
            db.HistorialAcademico.Add(HistorialAcademico.Crear(
                insc.EstudianteId, insc.MateriaId, insc.CursoId,
                Anio, curso.Comision, estado, nota, insc.Estudiante.Condicion));
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
        List<InscripcionMateria> inscripciones, CancellationToken ct)
    {
        foreach (var materia in materias)
        {
            var encuesta = Encuesta.Crear(
                $"Evaluación Docente 2023 — {materia.Nombre}",
                TipoEncuesta.EvaluacionDocente, Anio, materiaId: materia.Id);
            db.Encuestas.Add(encuesta);
            await db.SaveChangesAsync(ct);

            var p1 = PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo evaluás la claridad explicativa del docente?", 1, TipoPregunta.EscalaLikert);
            var p2 = PreguntaEncuesta.Crear(encuesta.Id, "¿Qué aspectos mejorarías de la cursada?", 2, TipoPregunta.TextoLibre, false);
            db.PreguntasEncuesta.AddRange(p1, p2);
            await db.SaveChangesAsync(ct);

            var respondentes = inscripciones
                .Where(im => im.MateriaId == materia.Id
                    && im.Estudiante.Condicion != CondicionEstudiante.Desertor)
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
        AppDbContext db, List<InscripcionMateria> inscripciones, CancellationToken ct)
    {
        foreach (var insc in inscripciones)
        {
            switch (insc.Estudiante.Condicion)
            {
                case CondicionEstudiante.Promocional:
                case CondicionEstudiante.Regular:
                case CondicionEstudiante.Egresado:
                    insc.MarcarAprobada();
                    break;
                case CondicionEstudiante.Libre:
                    insc.MarcarDesaprobada();
                    break;
                case CondicionEstudiante.Desertor:
                    insc.DarDeBaja();
                    break;
            }
        }
        await db.SaveChangesAsync(ct);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // Actualiza Estudiante.Anio para reflejar el año lectivo real en 2023.
    // Profesorado 2021 que cursó Año 3 → Anio = 3.
    private static async Task UpdateAnioEstudiantes(
        AppDbContext db, List<int> cursoIds2023, CancellationToken ct)
    {
        // Para cada año lectivo que tiene cursos en 2023, actualizar Estudiante.Anio
        // de todos los inscriptos (si su Anio actual es menor que el nivel del curso).
        var cursos2023 = await db.Cursos
            .Where(c => c.Anio == 2023)
            .ToListAsync(ct);

        foreach (var nivelGroup in cursos2023.GroupBy(c => c.AnioLectivo))
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
                    .Where(e => enNivel.Contains(e.Id) && e.Anio < nivel)
                    .ExecuteUpdateAsync(s => s.SetProperty(e => e.Anio, nivel), ct);
        }
    }
}
