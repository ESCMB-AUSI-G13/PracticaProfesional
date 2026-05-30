using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.ValueObjects;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Siembra todas las actividades académicas del año 2022:
///   EspaciosCurriculares, Asistencias (porcentaje por condición),
///   Exámenes (2 Parciales por materia), InscripcionesExamen con notas,
///   HistorialAcademico, Encuestas con respuestas anónimas,
///   y actualización del Estado final de InscripcionesMateria.
/// Idempotente: se omite si ya existen EspaciosCurriculares para cursos 2022.
/// </summary>
public static class Anio2022ActividadesSeeder
{
    private const int Anio = 2022;

    // 18 fechas de clase: 2 por mes, marzo–noviembre
    private static readonly DateTime[] FechasClase =
    [
        new(2022,3,8),  new(2022,3,22),
        new(2022,4,5),  new(2022,4,19),
        new(2022,5,3),  new(2022,5,17),
        new(2022,6,7),  new(2022,6,21),
        new(2022,7,5),  new(2022,7,19),
        new(2022,8,2),  new(2022,8,16),
        new(2022,9,6),  new(2022,9,20),
        new(2022,10,4), new(2022,10,18),
        new(2022,11,1), new(2022,11,15),
    ];

    private static readonly DateTime FechaParcial1 = new(2022, 5, 18);
    private static readonly DateTime FechaParcial2 = new(2022, 8, 17);
    private static readonly DateTime FechaParcial3 = new(2022, 10, 19);
    private static readonly DateTime FechaFinal    = new(2022, 12, 7);
    private static readonly DateTime FechaEncuesta = new(2022, 11, 20);

    private static readonly string[] TextosEncuesta =
    [
        "El ritmo de la clase es adecuado.",
        "Me gustaría más ejercicios prácticos.",
        "Excelente metodología de enseñanza.",
        "Podría mejorar la comunicación con los alumnos.",
        "Muy satisfecho con la cursada en general.",
    ];

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var cursoIds = await db.Cursos
            .Where(c => c.Anio == Anio)
            .Select(c => c.Id)
            .ToListAsync(ct);

        if (cursoIds.Count == 0)
        {
            logger.LogWarning("Anio2022ActividadesSeeder: no hay cursos de 2022.");
            return;
        }

        if (await db.EspaciosCurriculares.AnyAsync(ec => cursoIds.Contains(ec.CursoId), ct))
        {
            logger.LogInformation("Anio2022ActividadesSeeder: ya existe, omitido.");
            return;
        }

        var cursos     = await db.Cursos.Where(c => c.Anio == Anio).ToListAsync(ct);
        var materias   = await db.Materias.ToListAsync(ct);
        var docenteIds = await db.Docentes.Select(d => d.Id).OrderBy(d => d).ToListAsync(ct);

        var inscripciones = await db.InscripcionesMateria
            .Include(im => im.Estudiante)
            .Where(im => cursoIds.Contains(im.CursoId))
            .ToListAsync(ct);

        var materias2022 = materias
            .Where(m => inscripciones.Any(im => im.MateriaId == m.Id))
            .ToList();

        var materiaDocenteMap = await SeedEspaciosCurriculares(db, cursos, materias, docenteIds, ct);
        logger.LogInformation("Anio2022ActividadesSeeder: EspaciosCurriculares — OK.");

        await SeedAsistencias(db, inscripciones, ct);
        logger.LogInformation("Anio2022ActividadesSeeder: Asistencias — OK.");

        var examenesPorMateria = await SeedExamenes(db, materias2022, ct);
        logger.LogInformation("Anio2022ActividadesSeeder: Exámenes — OK.");

        await SeedInscripcionesExamen(db, inscripciones, examenesPorMateria, ct);
        logger.LogInformation("Anio2022ActividadesSeeder: InscripcionesExamen — OK.");

        await SeedHistorialAcademico(db, inscripciones, cursos, ct);
        logger.LogInformation("Anio2022ActividadesSeeder: HistorialAcademico — OK.");

        await SeedEncuestas(db, materias2022, inscripciones, ct);
        logger.LogInformation("Anio2022ActividadesSeeder: Encuestas — OK.");

        await UpdateEstados(db, inscripciones, ct);
        logger.LogInformation("Anio2022ActividadesSeeder: Estados — OK.");

        logger.LogInformation(
            "Anio2022ActividadesSeeder: completado — {N} inscripciones procesadas.", inscripciones.Count);
    }

    // ──────────────────────────────────────────────────────────
    private static async Task<Dictionary<int, int>> SeedEspaciosCurriculares(
        AppDbContext db, List<Curso> cursos, List<Materia> materias,
        List<int> docenteIds, CancellationToken ct)
    {
        // Mismo docente para la misma materia independientemente de la comisión
        var materias2022 = materias
            .Where(m => cursos.Any(c => c.CarreraId == m.CarreraId && c.AnioLectivo == m.Anio))
            .ToList();

        var map = new Dictionary<int, int>();
        int idx = 0;
        foreach (var m in materias2022)
            map[m.Id] = docenteIds[idx++ % docenteIds.Count];

        foreach (var curso in cursos)
        {
            foreach (var materia in materias.Where(m => m.CarreraId == curso.CarreraId && m.Anio == curso.AnioLectivo))
                db.EspaciosCurriculares.Add(EspacioCurricular.Crear(materia.Id, map[materia.Id], curso.Id));
        }

        await db.SaveChangesAsync(ct);
        return map;
    }

    // ──────────────────────────────────────────────────────────
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
        CondicionEstudiante.Regular     => idx % 3 != 2 ? EstadoAsistencia.Presente : EstadoAsistencia.Ausente,
        CondicionEstudiante.Libre       => idx < 9 ? EstadoAsistencia.Presente : EstadoAsistencia.Ausente,
        CondicionEstudiante.Desertor    => idx < 4 ? EstadoAsistencia.Presente : EstadoAsistencia.Ausente,
        _                               => EstadoAsistencia.Presente
    };

    // ──────────────────────────────────────────────────────────
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

    // ──────────────────────────────────────────────────────────
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
                // Promocionales no rinden el Final (ya están promovidos por cursada)
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
            (CondicionEstudiante.Promocional, _)              => 8 + seed % 3,   // 8-10
            (CondicionEstudiante.Regular,     TipoExamen.Final)  => 6 + seed % 3,   // 6-8 (aprueba el final)
            (CondicionEstudiante.Regular,     _)              => 5 + seed % 3,   // 5-7 (parciales)
            (CondicionEstudiante.Libre,       TipoExamen.Final)  => 2 + seed % 5,   // 2-6 (algunos pasan)
            (CondicionEstudiante.Libre,       _)              => 1 + seed % 4,   // 1-4 (parciales)
            _                                                 => 3
        };
    }

    // ──────────────────────────────────────────────────────────
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
        CondicionEstudiante.Regular     => ("Regularizado", (decimal)(4 + id % 4)),
        CondicionEstudiante.Libre       => ("Libre",        (decimal)(1 + id % 3)),
        CondicionEstudiante.Desertor    => ("Abandonó",     null),
        _                               => ("Regular",      null)
    };

    // ──────────────────────────────────────────────────────────
    private static async Task SeedEncuestas(
        AppDbContext db, List<Materia> materias,
        List<InscripcionMateria> inscripciones, CancellationToken ct)
    {
        foreach (var materia in materias)
        {
            var encuesta = Encuesta.Crear(
                $"Evaluación Docente 2022 — {materia.Nombre}",
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

    // ──────────────────────────────────────────────────────────
    private static async Task UpdateEstados(
        AppDbContext db, List<InscripcionMateria> inscripciones, CancellationToken ct)
    {
        foreach (var insc in inscripciones)
        {
            switch (insc.Estudiante.Condicion)
            {
                case CondicionEstudiante.Promocional:
                case CondicionEstudiante.Regular:
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
}
