using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Crea la encuesta de satisfacción general por defecto si no existe ninguna.
/// </summary>
public static class EncuestaSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (db.Encuestas.Any()) return;

        var encuesta = Encuesta.Crear(
            titulo:       "Evaluación Institucional 2026",
            tipo:         TipoEncuesta.SatisfaccionGeneral,
            cicloLectivo: 2026,
            descripcion:  "Encuesta de satisfacción estudiantil. Tus respuestas son completamente anónimas.");

        db.Encuestas.Add(encuesta);
        await db.SaveChangesAsync(ct);

        var preguntas = new[]
        {
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás la calidad de la enseñanza en general?",       1, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás la atención y disposición de los docentes?",   2, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás las instalaciones y recursos de la institución?", 3, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás la organización y comunicación institucional?", 4, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás tu experiencia general como estudiante?",       5, TipoPregunta.EscalaLikert),
            PreguntaEncuesta.Crear(encuesta.Id, "¿Qué aspectos mejorarías? (opcional)", 6, TipoPregunta.TextoLibre, esObligatoria: false),
        };

        db.PreguntasEncuesta.AddRange(preguntas);
        await db.SaveChangesAsync(ct);
    }
}
