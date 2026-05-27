using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Infrastructure.Seeding;

/// <summary>
/// Genera respuestas anónimas de prueba para todas las encuestas activas.
/// Usa la misma lógica de token SHA-256 que ResponderEncuestaUseCase.
/// Idempotente: si ya existen respuestas para una encuesta, la saltea.
/// </summary>
public static class EncuestaRespuestasSeeder
{
    private static readonly string[] ComentariosPositivos =
    [
        "Me parece que el nivel de los docentes es muy bueno y se nota el compromiso.",
        "Muy contento con la carrera. Los profesores explican bien y son accesibles.",
        "Excelente institución, muy ordenada y con buena comunicación.",
        "Los recursos disponibles son adecuados y el ambiente de estudio es agradable.",
        "La organización mejoró mucho este año, se nota el esfuerzo institucional.",
        "Buena propuesta académica. Recomendaría la institución sin dudas.",
        "Me siento bien acompañado en mi trayecto como estudiante.",
    ];

    private static readonly string[] ComentariosNeutros =
    [
        "Podría mejorarse la comunicación sobre fechas de exámenes con más anticipación.",
        "A veces falta más material de estudio actualizado en algunas materias.",
        "Las instalaciones son básicas pero funcionales. Estaría bueno mejorar el espacio.",
        "Hay docentes muy buenos y otros que podrían mejorar su didáctica.",
        "La carga horaria es alta pero es parte de la carrera. Estaría bueno más apoyo.",
        "Mejoraría la disponibilidad de horarios para consultas con docentes.",
    ];

    public static async Task SeedAsync(
        AppDbContext db, string salt = "pp-salt-2026", CancellationToken ct = default)
    {
        if (await db.RespuestasEncuesta.AnyAsync(ct)) return;

        var encuestas = await db.Encuestas
            .Where(e => e.Activa)
            .Include(e => e.Preguntas)
            .ToListAsync(ct);

        if (encuestas.Count == 0) return;

        var estudiantes = await db.Estudiantes.ToListAsync(ct);
        if (estudiantes.Count == 0) return;

        var rnd = new Random(42);
        var ahora = DateTime.UtcNow;

        foreach (var encuesta in encuestas)
        {
            foreach (var estudiante in estudiantes)
            {
                var token = ComputarToken(estudiante.Id, encuesta.Id, salt);

                if (await db.EncuestasCompletadas
                    .AnyAsync(ec => ec.TokenAnonimo == token && ec.EncuestaId == encuesta.Id, ct))
                    continue;

                // Fechas distribuidas en los últimos 3 meses para poblar evolución mensual
                var fecha = ahora.AddDays(-rnd.Next(0, 90));

                var respuesta = RespuestaEncuesta.Crear(encuesta.Id, fecha);
                db.RespuestasEncuesta.Add(respuesta);
                await db.SaveChangesAsync(ct);

                foreach (var pregunta in encuesta.Preguntas)
                {
                    if (!pregunta.EsObligatoria && rnd.NextDouble() < 0.35)
                        continue; // 35% de los estudiantes omite preguntas opcionales

                    ItemRespuesta item;
                    if (pregunta.TipoPregunta == TipoPregunta.EscalaLikert)
                    {
                        item = ItemRespuesta.Crear(respuesta.Id, pregunta.Id, GenerarLikert(rnd), null);
                    }
                    else
                    {
                        var textos = rnd.NextDouble() < 0.65 ? ComentariosPositivos : ComentariosNeutros;
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

    // Distribución realista: mayoría satisfechos, minoría críticos
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
