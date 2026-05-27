using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Domain.Entities;

public class PreguntaEncuesta
{
    public int          Id           { get; private set; }
    public int          EncuestaId   { get; private set; }
    public string       Texto        { get; private set; } = string.Empty;
    public int          Orden        { get; private set; }
    public TipoPregunta TipoPregunta { get; private set; }
    public bool         EsObligatoria { get; private set; }

    public Encuesta         Encuesta  { get; private set; } = null!;
    public ICollection<ItemRespuesta> Items { get; private set; } = new List<ItemRespuesta>();

    private PreguntaEncuesta() { }

    public static PreguntaEncuesta Crear(
        int encuestaId, string texto, int orden,
        TipoPregunta tipo, bool esObligatoria = true)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("El texto de la pregunta es obligatorio.");

        return new PreguntaEncuesta
        {
            EncuestaId    = encuestaId,
            Texto         = texto.Trim(),
            Orden         = orden,
            TipoPregunta  = tipo,
            EsObligatoria = esObligatoria
        };
    }
}
