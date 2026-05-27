namespace PracticaProfesional.Domain.Entities;

/// <summary>
/// Registro de que un estudiante completó una encuesta.
/// TokenAnonimo = SHA-256(estudianteId | encuestaId | salt) — no permite recuperar la identidad.
/// </summary>
public class EncuestaCompletada
{
    public int      Id             { get; private set; }
    public string   TokenAnonimo   { get; private set; } = string.Empty;
    public int      EncuestaId     { get; private set; }
    public DateTime FechaCompletada { get; private set; }

    public Encuesta Encuesta { get; private set; } = null!;

    private EncuestaCompletada() { }

    public static EncuestaCompletada Crear(string tokenAnonimo, int encuestaId)
        => new()
        {
            TokenAnonimo    = tokenAnonimo,
            EncuestaId      = encuestaId,
            FechaCompletada = DateTime.UtcNow
        };
}
