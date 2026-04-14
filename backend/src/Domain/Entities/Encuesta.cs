namespace PracticaProfesional.Domain.Entities;

public class Encuesta
{
    public int Id { get; private set; }
    public int MateriaId { get; private set; }
    public Materia Materia { get; private set; } = null!;
    public int DocenteId { get; private set; }
    public Docente Docente { get; private set; } = null!;
    public string Preguntas { get; private set; } = string.Empty;
    public bool Activa { get; private set; }
    public DateTime FechaCreacion { get; private set; }

    public ICollection<RespuestaEncuesta> Respuestas { get; private set; } = new List<RespuestaEncuesta>();

    private Encuesta() { }

    public static Encuesta Crear(int materiaId, int docenteId, string preguntas)
    {
        if (string.IsNullOrWhiteSpace(preguntas)) throw new ArgumentException("Las preguntas son obligatorias.");

        return new Encuesta
        {
            MateriaId = materiaId,
            DocenteId = docenteId,
            Preguntas = preguntas,
            Activa = true,
            FechaCreacion = DateTime.UtcNow
        };
    }

    public void Desactivar() => Activa = false;
}
