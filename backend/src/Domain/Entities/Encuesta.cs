using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Domain.Entities;

public class Encuesta
{
    public int         Id            { get; private set; }
    public string      Titulo        { get; private set; } = string.Empty;
    public string?     Descripcion   { get; private set; }
    public TipoEncuesta Tipo         { get; private set; }
    public int?        MateriaId     { get; private set; }   // Solo para EvaluacionDocente
    public int         CicloLectivo  { get; private set; }   // Año al que pertenece
    public bool        Activa        { get; private set; }
    public DateTime    FechaCreacion { get; private set; }

    public Materia?    Materia    { get; private set; }
    public ICollection<PreguntaEncuesta>  Preguntas  { get; private set; } = new List<PreguntaEncuesta>();
    public ICollection<RespuestaEncuesta> Respuestas { get; private set; } = new List<RespuestaEncuesta>();

    private Encuesta() { }

    public static Encuesta Crear(
        string titulo, TipoEncuesta tipo, int cicloLectivo,
        string? descripcion = null, int? materiaId = null)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new ArgumentException("El título es obligatorio.");
        if (tipo == TipoEncuesta.EvaluacionDocente && materiaId is null)
            throw new ArgumentException("La evaluación docente requiere una materia.");

        return new Encuesta
        {
            Titulo        = titulo.Trim(),
            Descripcion   = descripcion?.Trim(),
            Tipo          = tipo,
            MateriaId     = materiaId,
            CicloLectivo  = cicloLectivo,
            Activa        = true,
            FechaCreacion = DateTime.UtcNow
        };
    }

    public void Activar()   => Activa = true;
    public void Desactivar() => Activa = false;
}
