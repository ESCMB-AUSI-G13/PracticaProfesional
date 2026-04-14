namespace PracticaProfesional.Domain.Entities;

/// <summary>
/// Respuesta anónima a una encuesta. Sin FK al alumno (anonimización CU-36/CU-40).
/// </summary>
public class RespuestaEncuesta
{
    public int Id { get; private set; }
    public int EncuestaId { get; private set; }
    public Encuesta Encuesta { get; private set; } = null!;
    public string Preguntas { get; private set; } = string.Empty;
    public string Respuestas { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }

    private RespuestaEncuesta() { }

    public static RespuestaEncuesta Crear(int encuestaId, string preguntas, string respuestas)
    {
        if (string.IsNullOrWhiteSpace(respuestas)) throw new ArgumentException("Las respuestas son obligatorias.");

        return new RespuestaEncuesta
        {
            EncuestaId = encuestaId,
            Preguntas = preguntas,
            Respuestas = respuestas,
            Fecha = DateTime.UtcNow
        };
    }
}
