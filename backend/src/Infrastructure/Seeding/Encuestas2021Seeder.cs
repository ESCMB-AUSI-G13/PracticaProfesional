using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Crea encuestas del ciclo lectivo 2021 y genera respuestas anónimas coherentes.
///
/// Encuestas creadas:
///   3 × SatisfaccionGeneral (institucional, pedagógica, gestión)
///   1 × EvaluacionDocente por cada materia de 1er año (16 total)
///   Total: 19 encuestas.
///
/// Coherencia: el puntaje Likert de cada EvaluacionDocente es proporcional
/// al promedio de notas de esa materia en 2021 — las materias con menor
/// rendimiento reciben valoraciones más críticas.
///
/// Solo responden los estudiantes activos en 2021 (Promocional, Regular, Libre).
/// Los desertores de año 1 no responden.
/// Tasa de respuesta: ~70%.
/// </summary>
public static class Encuestas2021Seeder
{
    private const string Salt = "pp-salt-2021";

    // ── Comentarios para texto libre ─────────────────────────────────────────

    private static readonly string[] DocentePos =
    [
        "El docente explica muy bien los contenidos, se nota la preparación.",
        "Muy buena disposición para consultas, siempre encuentra la forma de explicar.",
        "Las clases son dinámicas y se trabaja bien el material teórico y práctico.",
        "Muy buen ritmo de clase, se avanza con claridad sin dejar dudas.",
        "El docente genera un ambiente de confianza que facilita el aprendizaje.",
    ];

    private static readonly string[] DocenteNeu =
    [
        "La materia tiene mucho contenido para el tiempo disponible.",
        "Podría darse más tiempo a la práctica además de la teoría.",
        "El docente explica bien, aunque a veces avanza rápido.",
        "Las evaluaciones son exigentes, estaría bueno una guía de estudio más completa.",
        "Buen docente, pero el material bibliográfico podría estar más actualizado.",
    ];

    private static readonly string[] DocenteNeg =
    [
        "La materia es muy difícil y no se siente suficiente acompañamiento.",
        "Cuesta seguir el ritmo de la clase, faltaría más apoyo para quienes tienen dificultades.",
        "Las explicaciones no siempre son claras, hay que recurrir mucho al material por cuenta propia.",
        "El nivel de exigencia es muy alto para el tiempo de cursada disponible.",
        "Sería bueno tener más instancias de consulta antes de los parciales.",
    ];

    private static readonly string[] InstPos =
    [
        "Muy conforme con la propuesta de la institución, se nota el compromiso.",
        "El nivel académico es bueno y los docentes están bien preparados.",
        "La institución ofrece una formación sólida, estoy muy satisfecho/a.",
        "Buen ambiente de estudio y buena organización del año lectivo.",
        "Recomendaría la institución sin dudas a futuros estudiantes.",
    ];

    private static readonly string[] InstNeu =
    [
        "Podría mejorarse la comunicación de fechas y novedades institucionales.",
        "La carga académica es alta, estaría bueno más espacios de apoyo al estudiante.",
        "Hay docentes muy buenos y otros que podrían mejorar su didáctica.",
        "Las instalaciones son funcionales pero podrían mejorar.",
        "En general conforme, aunque hay aspectos a mejorar en la organización.",
    ];

    private static readonly string[] AdminPos =
    [
        "La atención en secretaría es rápida y eficiente.",
        "Los trámites se resuelven bien y en tiempo razonable.",
        "La preceptoría está muy bien organizada y es accesible.",
        "El calendario académico estuvo bien comunicado durante el año.",
    ];

    private static readonly string[] AdminNeu =
    [
        "A veces los certificados tardan en estar listos.",
        "Sería bueno ampliar el horario de atención en secretaría.",
        "La comunicación podría ser más fluida en momentos de exámenes.",
        "Las aulas son funcionales pero necesitan mejoras de mantenimiento.",
    ];

    // ── Entry point ───────────────────────────────────────────────────────────

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        bool yaExisten = await db.Encuestas.AnyAsync(e => e.CicloLectivo == 2021, ct);
        if (yaExisten)
        {
            logger.LogInformation("Encuestas2021Seeder: ya existen encuestas de 2021, omitido.");
            return;
        }

        // Materias de 1er año de ambas carreras
        var materias = await db.Materias
            .Where(m => m.Anio == 1)
            .OrderBy(m => m.CarreraId).ThenBy(m => m.Id)
            .ToListAsync(ct);

        if (materias.Count == 0)
        {
            logger.LogWarning("Encuestas2021Seeder: no hay materias de 1er año.");
            return;
        }

        // Crear encuestas institucionales
        var encInstitucional = await CrearConPreguntasAsync(db,
            "Evaluación Institucional 2021",
            "Encuesta de satisfacción estudiantil 2021. Tus respuestas son completamente anónimas.",
            TipoEncuesta.SatisfaccionGeneral, 2021, null,
            [
                ("¿Cómo calificás la calidad de la enseñanza en general?",          TipoPregunta.EscalaLikert, true),
                ("¿Cómo calificás la atención y disposición de los docentes?",      TipoPregunta.EscalaLikert, true),
                ("¿Cómo calificás las instalaciones y recursos de la institución?", TipoPregunta.EscalaLikert, true),
                ("¿Cómo calificás la organización y comunicación institucional?",   TipoPregunta.EscalaLikert, true),
                ("¿Cómo calificás tu experiencia general como estudiante?",         TipoPregunta.EscalaLikert, true),
                ("¿Qué aspectos mejorarías? (opcional)",                            TipoPregunta.TextoLibre,   false),
            ], ct);

        var encPedagogica = await CrearConPreguntasAsync(db,
            "Propuesta Pedagógica 2021",
            "Encuesta sobre la propuesta de enseñanza 2021. Anónima y confidencial.",
            TipoEncuesta.SatisfaccionGeneral, 2021, null,
            [
                ("¿Los contenidos del año fueron pertinentes para tu formación?",        TipoPregunta.EscalaLikert, true),
                ("¿El nivel de exigencia fue adecuado para los tiempos de cursada?",     TipoPregunta.EscalaLikert, true),
                ("¿Contaste con los recursos bibliográficos necesarios?",                TipoPregunta.EscalaLikert, true),
                ("¿Las evaluaciones reflejaron lo trabajado en clase?",                  TipoPregunta.EscalaLikert, true),
                ("¿Qué aspecto de la propuesta pedagógica mejorarías? (opcional)",       TipoPregunta.TextoLibre,   false),
            ], ct);

        var encGestion = await CrearConPreguntasAsync(db,
            "Gestión y Organización 2021",
            "Encuesta sobre procesos administrativos e infraestructura 2021. Anónima.",
            TipoEncuesta.SatisfaccionGeneral, 2021, null,
            [
                ("¿Cómo calificás la atención en secretaría y preceptoría?",                     TipoPregunta.EscalaLikert, true),
                ("¿Cómo calificás los tiempos de respuesta ante trámites?",                      TipoPregunta.EscalaLikert, true),
                ("¿Cómo calificás el acceso a información sobre el calendario académico?",        TipoPregunta.EscalaLikert, true),
                ("¿Cómo calificás el estado de las aulas y espacios comunes?",                   TipoPregunta.EscalaLikert, true),
                ("¿Qué aspecto administrativo o de infraestructura mejorarías? (opcional)",      TipoPregunta.TextoLibre,   false),
            ], ct);

        // Crear EvaluacionDocente por materia
        var encuestasPorMateria = new Dictionary<int, Encuesta>(); // materiaId → encuesta
        foreach (var materia in materias)
        {
            var enc = await CrearConPreguntasAsync(db,
                $"Evaluación Docente 2021 — {materia.Nombre}",
                $"Evaluación de la práctica docente en {materia.Nombre} 2021. Anónima.",
                TipoEncuesta.EvaluacionDocente, 2021, materia.Id,
                [
                    ("¿Cómo calificás la claridad en las explicaciones del/la docente?",           TipoPregunta.EscalaLikert, true),
                    ("¿Cómo calificás la predisposición para responder consultas?",                 TipoPregunta.EscalaLikert, true),
                    ("¿Cómo calificás la organización y planificación de la materia?",              TipoPregunta.EscalaLikert, true),
                    ("¿Los contenidos tienen dificultad adecuada al tiempo disponible?",            TipoPregunta.EscalaLikert, true),
                    ("¿El/la docente fomenta la participación y el pensamiento crítico?",           TipoPregunta.EscalaLikert, true),
                    ("¿Qué aspectos de la materia o del/la docente mejorarías? (opcional)",        TipoPregunta.TextoLibre,   false),
                ], ct);

            encuestasPorMateria[materia.Id] = enc;
        }

        logger.LogInformation("Encuestas2021Seeder: {N} encuestas creadas.", 3 + encuestasPorMateria.Count);

        // Calcular promedio de notas por materia (para coherencia de respuestas)
        var promediosPorMateria = await db.InscripcionesExamen
            .Where(ie => ie.NotaValor != null
                      && db.Examenes
                           .Where(e => e.FechaExamen.Year == 2021 && e.TipoExamen == TipoExamen.Parcial)
                           .Select(e => e.Id)
                           .Contains(ie.ExamenId))
            .GroupBy(ie => ie.Examen.MateriaId)
            .Select(g => new { MateriaId = g.Key, Promedio = g.Average(ie => ie.NotaValor!.Value) })
            .ToDictionaryAsync(x => x.MateriaId, x => (double)x.Promedio, ct);

        // Estudiantes activos en 2021 (Promo + Regular + Libre)
        var cursosIds2021 = await db.Cursos
            .Where(c => c.Anio == 2021 && c.AnioLectivo == 1)
            .Select(c => c.Id)
            .ToListAsync(ct);

        var estudiantesActivos = await db.Estudiantes
            .Where(e => e.FechaDeIngreso.Year == 2021
                     && e.Condicion != CondicionEstudiante.Desertor)
            .ToListAsync(ct);

        if (estudiantesActivos.Count == 0)
        {
            logger.LogWarning("Encuestas2021Seeder: sin estudiantes activos, no se generan respuestas.");
            return;
        }

        var rng = new Random(2021);
        int totalRespuestas = 0;
        double avgGlobal = promediosPorMateria.Values.DefaultIfEmpty(5.0).Average();

        // Respuestas institucionales
        foreach (var encuesta in new[] { encInstitucional, encPedagogica, encGestion })
        {
            var preguntas = await db.PreguntasEncuesta
                .Where(p => p.EncuestaId == encuesta.Id)
                .ToListAsync(ct);

            totalRespuestas += await GenerarRespuestasBatchAsync(
                db, encuesta, preguntas, estudiantesActivos,
                tasaRespuesta: 0.72, avgNota: avgGlobal,
                comentariosPos: InstPos, comentariosNeg: InstNeu, rng, ct);
        }

        // Respuestas por materia (EvaluacionDocente)
        foreach (var (materiaId, encuesta) in encuestasPorMateria)
        {
            var preguntas = await db.PreguntasEncuesta
                .Where(p => p.EncuestaId == encuesta.Id)
                .ToListAsync(ct);

            var inscriptosIds = (await db.InscripcionesMateria
                .Where(im => im.MateriaId == materiaId
                          && cursosIds2021.Contains(im.CursoId)
                          && (im.Estado == EstadoInscripcion.Aprobada
                           || im.Estado == EstadoInscripcion.Desaprobada))
                .Select(im => im.EstudianteId)
                .ToListAsync(ct)).ToHashSet();

            var estudiantesMateria = estudiantesActivos
                .Where(e => inscriptosIds.Contains(e.Id))
                .ToList();

            double avgNota = promediosPorMateria.TryGetValue(materiaId, out var avg) ? avg : 5.0;

            totalRespuestas += await GenerarRespuestasBatchAsync(
                db, encuesta, preguntas, estudiantesMateria,
                tasaRespuesta: 0.68, avgNota,
                comentariosPos: DocentePos,
                comentariosNeg: avgNota >= 5.0 ? DocenteNeu : DocenteNeg,
                rng, ct);
        }

        logger.LogInformation(
            "Encuestas2021Seeder: {R} respuestas generadas para {E} estudiantes activos.",
            totalRespuestas, estudiantesActivos.Count);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task<Encuesta> CrearConPreguntasAsync(
        AppDbContext db,
        string titulo, string descripcion,
        TipoEncuesta tipo, int cicloLectivo, int? materiaId,
        (string Texto, TipoPregunta TipoPregunta, bool EsObligatoria)[] preguntas,
        CancellationToken ct)
    {
        var encuesta = Encuesta.Crear(titulo, tipo, cicloLectivo, descripcion, materiaId);
        encuesta.Desactivar(); // históricas → cerradas
        db.Encuestas.Add(encuesta);
        await db.SaveChangesAsync(ct);

        for (int i = 0; i < preguntas.Length; i++)
        {
            var (texto, tipoPregunta, esObligatoria) = preguntas[i];
            db.PreguntasEncuesta.Add(
                PreguntaEncuesta.Crear(encuesta.Id, texto, i + 1, tipoPregunta, esObligatoria));
        }
        await db.SaveChangesAsync(ct);
        return encuesta;
    }

    /// <summary>
    /// Genera respuestas para una encuesta en batch:
    /// - Guarda RespuestaEncuesta de a una (necesita ID para los ítems)
    /// - Acumula ItemsRespuesta y EncuestasCompletadas, guarda todo al final
    /// Esto reduce los round-trips a la BD de 3×N a N+1.
    /// </summary>
    private static async Task<int> GenerarRespuestasBatchAsync(
        AppDbContext db,
        Encuesta encuesta,
        List<PreguntaEncuesta> preguntas,
        List<Estudiante> estudiantes,
        double tasaRespuesta,
        double avgNota,
        string[] comentariosPos,
        string[] comentariosNeg,
        Random rng,
        CancellationToken ct)
    {
        var items   = new List<ItemRespuesta>();
        var tokens  = new List<EncuestaCompletada>();
        int creadas = 0;

        foreach (var est in estudiantes)
        {
            if (rng.NextDouble() > tasaRespuesta) continue;

            var token = ComputarToken(est.Id, encuesta.Id, Salt);
            var mes   = rng.Next(9, 12);
            var dia   = rng.Next(1, 28);
            var fecha = new DateTime(2021, mes, dia, 0, 0, 0, DateTimeKind.Utc);

            // Guardar la respuesta para obtener su ID
            var respuesta = RespuestaEncuesta.Crear(encuesta.Id, fecha);
            db.RespuestasEncuesta.Add(respuesta);
            await db.SaveChangesAsync(ct);

            // Acumular ítems sin guardar aún
            foreach (var pregunta in preguntas)
            {
                if (!pregunta.EsObligatoria && rng.NextDouble() < 0.35)
                    continue;

                ItemRespuesta item;
                if (pregunta.TipoPregunta == TipoPregunta.EscalaLikert)
                {
                    item = ItemRespuesta.Crear(respuesta.Id, pregunta.Id,
                        GenerarLikertCoherente(rng, avgNota), null);
                }
                else
                {
                    bool usarPos = rng.NextDouble() < ScoreRatio(avgNota);
                    var banco    = usarPos ? comentariosPos : comentariosNeg;
                    item = ItemRespuesta.Crear(respuesta.Id, pregunta.Id, null,
                        banco[rng.Next(banco.Length)]);
                }
                items.Add(item);
            }

            tokens.Add(EncuestaCompletada.Crear(token, encuesta.Id));
            creadas++;
        }

        // Un único save para todos los ítems y tokens de esta encuesta
        if (items.Count > 0 || tokens.Count > 0)
        {
            db.ItemsRespuesta.AddRange(items);
            db.EncuestasCompletadas.AddRange(tokens);
            await db.SaveChangesAsync(ct);
        }

        return creadas;
    }

    /// <summary>
    /// Genera un score Likert (1-5) sesgado según el promedio de nota de la materia.
    /// avgNota: 1-10 → a mayor nota, mayor score Likert.
    /// </summary>
    private static int GenerarLikertCoherente(Random rng, double avgNota)
    {
        double r = rng.NextDouble();

        if (avgNota < 3.5)      // Materia difícil → respuestas muy críticas
            return r < 0.20 ? 1 : r < 0.50 ? 2 : r < 0.75 ? 3 : r < 0.92 ? 4 : 5;
        else if (avgNota < 5.5) // Materia con dificultad media → respuestas mixtas
            return r < 0.05 ? 1 : r < 0.18 ? 2 : r < 0.42 ? 3 : r < 0.75 ? 4 : 5;
        else if (avgNota < 7.5) // Materia manejable → respuestas positivas
            return r < 0.02 ? 1 : r < 0.07 ? 2 : r < 0.22 ? 3 : r < 0.60 ? 4 : 5;
        else                    // Materia fácil → respuestas muy positivas
            return r < 0.01 ? 1 : r < 0.04 ? 2 : r < 0.13 ? 3 : r < 0.45 ? 4 : 5;
    }

    /// <summary>Probabilidad de usar comentario positivo según rendimiento (0-1).</summary>
    private static double ScoreRatio(double avgNota) =>
        Math.Clamp((avgNota - 1.0) / 9.0, 0.1, 0.9);

    private static string ComputarToken(int estudianteId, int encuestaId, string salt)
    {
        var raw   = $"{estudianteId}|{encuestaId}|{salt}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
