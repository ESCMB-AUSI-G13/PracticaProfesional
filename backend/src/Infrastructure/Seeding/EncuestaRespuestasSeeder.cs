using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Genera respuestas anónimas de prueba para encuestas activas sin respuestas aún.
/// - EvaluacionDocente: solo estudiantes con inscripción activa en la materia de la encuesta.
/// - SatisfaccionGeneral: todos los estudiantes.
/// Idempotente por encuesta: si ya tiene respuestas, la saltea.
/// </summary>
public static class EncuestaRespuestasSeeder
{
    private static readonly string[] ComentariosInstitucionalPos =
    [
        "Me parece que el nivel de los docentes es muy bueno y se nota el compromiso.",
        "Muy contento con la carrera. Los profesores explican bien y son accesibles.",
        "Excelente institución, muy ordenada y con buena comunicación.",
        "Los recursos disponibles son adecuados y el ambiente de estudio es agradable.",
        "La organización mejoró mucho este año, se nota el esfuerzo institucional.",
        "Buena propuesta académica. Recomendaría la institución sin dudas.",
        "Me siento bien acompañado en mi trayecto como estudiante.",
    ];

    private static readonly string[] ComentariosInstitucionalNeu =
    [
        "Podría mejorarse la comunicación sobre fechas de exámenes con más anticipación.",
        "A veces falta más material de estudio actualizado en algunas materias.",
        "Las instalaciones son básicas pero funcionales. Estaría bueno mejorar el espacio.",
        "Hay docentes muy buenos y otros que podrían mejorar su didáctica.",
        "La carga horaria es alta pero es parte de la carrera. Estaría bueno más apoyo.",
        "Mejoraría la disponibilidad de horarios para consultas con docentes.",
    ];

    private static readonly string[] ComentariosDocentePos =
    [
        "El docente explica muy bien y siempre está disponible para consultas.",
        "Excelente manejo del grupo, genera un clima de aprendizaje muy bueno.",
        "Las clases son dinámicas e interesantes, se nota la preparación.",
        "Muy buen docente, claro y preciso en sus explicaciones.",
        "Se preocupa por que todos entiendan, no avanza hasta que queda claro.",
        "Muy accesible, responde bien las dudas tanto en clase como por mail.",
    ];

    private static readonly string[] ComentariosDocenteNeu =
    [
        "A veces va muy rápido con los temas, estaría bueno más tiempo para práctica.",
        "El material podría estar más actualizado.",
        "Las evaluaciones son exigentes pero justas.",
        "Podría mejorar la devolución de los trabajos prácticos.",
        "Buen docente, aunque a veces le cuesta manejar los tiempos de la clase.",
    ];

    private static readonly string[] ComentariosAdminPos =
    [
        "La atención en secretaría es muy buena, siempre me respondieron rápido.",
        "Los trámites se resuelven en tiempo y forma, estoy muy conforme.",
        "El calendario académico está bien comunicado, no tuve problemas para organizarme.",
        "Las aulas están en buen estado y tienen lo necesario para dar clases.",
        "La preceptoría atiende muy bien, siempre dispuestos a ayudar.",
        "La gestión administrativa mejoró mucho respecto a años anteriores.",
    ];

    private static readonly string[] ComentariosAdminNeu =
    [
        "A veces los certificados tardan más de lo esperado en estar listos.",
        "Sería bueno que el calendario se publique con más anticipación.",
        "Las aulas son funcionales pero podrían mejorar la ventilación y el equipamiento.",
        "En horario pico la secretaría tiene mucha espera. Estaría bueno ampliar horarios.",
        "Mejoraría la señalización dentro del edificio para nuevos estudiantes.",
        "Algunos trámites podrían hacerse en línea para evitar ir hasta la institución.",
    ];

    public static async Task SeedAsync(
        AppDbContext db, string salt = "pp-salt-2026", CancellationToken ct = default)
    {
        var encuestas = await db.Encuestas
            .Where(e => e.Activa)
            .Include(e => e.Preguntas)
            .ToListAsync(ct);

        if (encuestas.Count == 0) return;

        // Limpiar respuestas incorrectas de EvaluacionDocente (generadas sin respetar inscripciones)
        await LimpiarRespuestasDocenteIncoherentesAsync(db, encuestas, salt, ct);

        var todosEstudiantes = await db.Estudiantes.ToListAsync(ct);
        if (todosEstudiantes.Count == 0) return;

        var rnd   = new Random(42);
        var ahora = DateTime.UtcNow;

        foreach (var encuesta in encuestas)
        {
            if (await db.RespuestasEncuesta.AnyAsync(r => r.EncuestaId == encuesta.Id, ct))
                continue;

            var estudiantesEncuesta = encuesta.Tipo == TipoEncuesta.EvaluacionDocente && encuesta.MateriaId.HasValue
                ? await ObtenerEstudiantesInscriptosAsync(db, encuesta.MateriaId.Value, ct)
                : todosEstudiantes;

            if (estudiantesEncuesta.Count == 0) continue;

            var (comentariosPos, comentariosNeu) = ElegirComentarios(encuesta.Tipo, encuesta.Titulo);

            // Distribuir respuestas desde el inicio del ciclo lectivo (1 de marzo) hasta hoy
            var inicioCiclo   = new DateTime(encuesta.CicloLectivo, 3, 1, 0, 0, 0, DateTimeKind.Utc);
            var diasDisponibles = Math.Max(1, (int)(ahora - inicioCiclo).TotalDays);

            foreach (var estudiante in estudiantesEncuesta)
            {
                var token = ComputarToken(estudiante.Id, encuesta.Id, salt);

                if (await db.EncuestasCompletadas
                    .AnyAsync(ec => ec.TokenAnonimo == token && ec.EncuestaId == encuesta.Id, ct))
                    continue;

                var fecha    = inicioCiclo.AddDays(rnd.Next(0, diasDisponibles));
                var respuesta = RespuestaEncuesta.Crear(encuesta.Id, fecha);
                db.RespuestasEncuesta.Add(respuesta);
                await db.SaveChangesAsync(ct);

                foreach (var pregunta in encuesta.Preguntas)
                {
                    if (!pregunta.EsObligatoria && rnd.NextDouble() < 0.35)
                        continue;

                    ItemRespuesta item;
                    if (pregunta.TipoPregunta == TipoPregunta.EscalaLikert)
                    {
                        item = ItemRespuesta.Crear(respuesta.Id, pregunta.Id, GenerarLikert(rnd), null);
                    }
                    else
                    {
                        var textos = rnd.NextDouble() < 0.65 ? comentariosPos : comentariosNeu;
                        var texto  = textos[rnd.Next(textos.Length)];
                        item = ItemRespuesta.Crear(respuesta.Id, pregunta.Id, null, texto);
                    }

                    db.ItemsRespuesta.Add(item);
                }

                db.EncuestasCompletadas.Add(EncuestaCompletada.Crear(token, encuesta.Id));
                await db.SaveChangesAsync(ct);
            }
        }
    }

    // Elimina respuestas de encuestas EvaluacionDocente que sean incoherentes:
    // - más respuestas que estudiantes inscriptos, O
    // - respuestas con fecha anterior al inicio del ciclo lectivo (1 de marzo).
    private static async Task LimpiarRespuestasDocenteIncoherentesAsync(
        AppDbContext db, List<Encuesta> encuestas, string salt, CancellationToken ct)
    {
        foreach (var encuesta in encuestas.Where(e =>
            e.Tipo == TipoEncuesta.EvaluacionDocente && e.MateriaId.HasValue))
        {
            var totalRespuestas = await db.RespuestasEncuesta
                .CountAsync(r => r.EncuestaId == encuesta.Id, ct);

            if (totalRespuestas == 0) continue;

            var inicioCiclo = new DateTime(encuesta.CicloLectivo, 3, 1, 0, 0, 0, DateTimeKind.Utc);

            var estudiantesInscriptos = await ObtenerEstudiantesInscriptosAsync(
                db, encuesta.MateriaId!.Value, ct);

            var tieneRespuestasAntesCiclo = await db.RespuestasEncuesta
                .AnyAsync(r => r.EncuestaId == encuesta.Id && r.Fecha < inicioCiclo, ct);

            var exceedeInscriptos = totalRespuestas > estudiantesInscriptos.Count;

            if (!tieneRespuestasAntesCiclo && !exceedeInscriptos) continue;

            var respuestas = await db.RespuestasEncuesta
                .Include(r => r.Items)
                .Where(r => r.EncuestaId == encuesta.Id)
                .ToListAsync(ct);

            db.ItemsRespuesta.RemoveRange(respuestas.SelectMany(r => r.Items));
            db.RespuestasEncuesta.RemoveRange(respuestas);

            var tokens = await db.EncuestasCompletadas
                .Where(ec => ec.EncuestaId == encuesta.Id)
                .ToListAsync(ct);
            db.EncuestasCompletadas.RemoveRange(tokens);

            await db.SaveChangesAsync(ct);
        }
    }

    private static async Task<List<Estudiante>> ObtenerEstudiantesInscriptosAsync(
        AppDbContext db, int materiaId, CancellationToken ct)
    {
        return await db.InscripcionesMateria
            .Where(i => i.MateriaId == materiaId
                     && (i.Estado == EstadoInscripcion.Activa
                      || i.Estado == EstadoInscripcion.Aprobada
                      || i.Estado == EstadoInscripcion.Desaprobada))
            .Select(i => i.Estudiante)
            .Distinct()
            .ToListAsync(ct);
    }

    private static (string[] Pos, string[] Neu) ElegirComentarios(TipoEncuesta tipo, string titulo) =>
        tipo == TipoEncuesta.EvaluacionDocente
            ? (ComentariosDocentePos, ComentariosDocenteNeu)
            : titulo.Contains("Administrativa")
                ? (ComentariosAdminPos, ComentariosAdminNeu)
                : (ComentariosInstitucionalPos, ComentariosInstitucionalNeu);

    private static int GenerarLikert(Random rnd) => rnd.NextDouble() switch
    {
        < 0.04 => 1,
        < 0.11 => 2,
        < 0.24 => 3,
        < 0.56 => 4,
        _      => 5
    };

    private static string ComputarToken(int estudianteId, int encuestaId, string salt)
    {
        var raw   = $"{estudianteId}|{encuestaId}|{salt}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
