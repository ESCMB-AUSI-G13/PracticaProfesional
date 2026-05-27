namespace PracticaProfesional.Domain.Entities;

public class ItemRespuesta
{
    public int     Id                 { get; private set; }
    public int     RespuestaEncuestaId { get; private set; }
    public int     PreguntaId         { get; private set; }
    public int?    ValorNumerico       { get; private set; }   // 1-5 para EscalaLikert
    public string? TextoLibre         { get; private set; }

    public RespuestaEncuesta Respuesta { get; private set; } = null!;
    public PreguntaEncuesta  Pregunta  { get; private set; } = null!;

    private ItemRespuesta() { }

    public static ItemRespuesta Crear(
        int respuestaEncuestaId, int preguntaId,
        int? valorNumerico, string? textoLibre)
        => new()
        {
            RespuestaEncuestaId = respuestaEncuestaId,
            PreguntaId          = preguntaId,
            ValorNumerico       = valorNumerico,
            TextoLibre          = textoLibre?.Trim()
        };
}
