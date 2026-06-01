using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

public static class EncuestaSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        var titulosExistentes = await db.Encuestas
            .Select(e => e.Titulo)
            .ToListAsync(ct);

        await SeedEvaluacionInstitucionalAsync(db, titulosExistentes, ct);
        await SeedEvaluacionDocenteAsync(db, titulosExistentes, ct);
        await SeedGestionAdministrativaAsync(db, titulosExistentes, ct);
    }

    // ── Encuesta 1: Satisfacción general institucional ────────────────────────

    private static async Task SeedEvaluacionInstitucionalAsync(
        AppDbContext db, List<string> existentes, CancellationToken ct)
    {
        const string titulo = "Evaluación Institucional 2026";
        if (existentes.Contains(titulo)) return;

        var encuesta = Encuesta.Crear(
            titulo:       titulo,
            tipo:         TipoEncuesta.SatisfaccionGeneral,
            cicloLectivo: 2026,
            descripcion:  "Encuesta de satisfacción estudiantil. Tus respuestas son completamente anónimas.");

        db.Encuestas.Add(encuesta);
        await db.SaveChangesAsync(ct);

        db.PreguntasEncuesta.AddRange(
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás la calidad de la enseñanza en general?",           1, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás la atención y disposición de los docentes?",       2, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás las instalaciones y recursos de la institución?",  3, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás la organización y comunicación institucional?",    4, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás tu experiencia general como estudiante?",          5, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Qué aspectos mejorarías? (opcional)",                             6, TipoPregunta.TextoLibre, esObligatoria: false)
        );
        await db.SaveChangesAsync(ct);
    }

    // ── Encuesta 2: Evaluación docente (vinculada a la primera materia del plan) ─

    private static async Task SeedEvaluacionDocenteAsync(
        AppDbContext db, List<string> existentes, CancellationToken ct)
    {
        const string titulo = "Evaluación Docente 2026";
        if (existentes.Contains(titulo)) return;

        // Usa la primera materia disponible; si no hay ninguna, omite el seed.
        var materia = await db.Materias.OrderBy(m => m.Id).FirstOrDefaultAsync(ct);
        if (materia is null) return;

        var encuesta = Encuesta.Crear(
            titulo:       titulo,
            tipo:         TipoEncuesta.EvaluacionDocente,
            cicloLectivo: 2026,
            descripcion:  "Evaluación de la práctica docente. Tus respuestas son anónimas.",
            materiaId:    materia.Id);

        encuesta.Desactivar(); // pendiente de configurar — activar desde el panel /encuestas
        db.Encuestas.Add(encuesta);
        await db.SaveChangesAsync(ct);

        db.PreguntasEncuesta.AddRange(
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás la claridad en las explicaciones del/la docente?",                          1, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás la predisposición del/la docente para responder consultas?",                 2, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás la organización y planificación de la materia?",                            3, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Los contenidos tienen una dificultad adecuada al tiempo disponible?",                      4, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿El/la docente fomenta la participación y el pensamiento crítico?",                          5, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿La/el docente mantuvo coherencia entre los temas desarrollados y los temas evaluados?",     6, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿La/el docente explicitó previamente los criterios de evaluación?",                         7, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿La/el docente entregó los resultados de las evaluaciones en tiempo adecuado?",              8, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿La/el docente cumplió con los horarios previstos?",                                        9, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Qué aspectos de la materia o del/la docente mejorarías? (opcional)",                      10, TipoPregunta.TextoLibre, esObligatoria: false)
        );
        await db.SaveChangesAsync(ct);
    }

    // ── Encuesta 3: Gestión administrativa ────────────────────────────────────

    private static async Task SeedGestionAdministrativaAsync(
        AppDbContext db, List<string> existentes, CancellationToken ct)
    {
        const string titulo = "Gestión Administrativa 2026";
        if (existentes.Contains(titulo)) return;

        var encuesta = Encuesta.Crear(
            titulo:       titulo,
            tipo:         TipoEncuesta.SatisfaccionGeneral,
            cicloLectivo: 2026,
            descripcion:  "Encuesta sobre procesos administrativos e infraestructura. Tus respuestas son anónimas.");

        db.Encuestas.Add(encuesta);
        await db.SaveChangesAsync(ct);

        db.PreguntasEncuesta.AddRange(
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás la atención en secretaría y preceptoría?",                              1, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás los tiempos de respuesta ante trámites (certificados, actas)?",          2, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás el acceso a información sobre fechas y calendario académico?",           3, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás el estado de las aulas y espacios comunes?",                            4, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Qué aspecto administrativo o de infraestructura mejorarías? (opcional)",               5, TipoPregunta.TextoLibre, esObligatoria: false)
        );
        await db.SaveChangesAsync(ct);
    }
}
