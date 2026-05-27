namespace PracticaProfesional.Domain.Entities;

/// <summary>
/// Respuesta anónima a una encuesta. Sin FK al estudiante (anonimización CU-36/CU-40).
/// La disociación de identidad se garantiza mediante token SHA-256 en EncuestaCompletada.
/// </summary>
public class RespuestaEncuesta
{
    public int      Id          { get; private set; }
    public int      EncuestaId  { get; private set; }
    public DateTime Fecha       { get; private set; }

    public Encuesta                  Encuesta { get; private set; } = null!;
    public ICollection<ItemRespuesta> Items   { get; private set; } = new List<ItemRespuesta>();

    private RespuestaEncuesta() { }

    public static RespuestaEncuesta Crear(int encuestaId, DateTime? fecha = null)
        => new() { EncuestaId = encuestaId, Fecha = fecha ?? DateTime.UtcNow };
}
