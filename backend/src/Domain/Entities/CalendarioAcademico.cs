using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Domain.Entities;

public class CalendarioAcademico
{
    public int Id { get; private set; }
    public string NombreEvento { get; private set; } = string.Empty;
    public string Comision { get; private set; } = string.Empty;
    public DateTime FechaInicio { get; private set; }
    public DateTime FechaFin { get; private set; }
    public TipoEvento TipoEvento { get; private set; }
    public int? MateriaId { get; private set; }
    public Materia? Materia { get; private set; }
    public int? CursoId { get; private set; }
    public Curso? Curso { get; private set; }

    private CalendarioAcademico() { }

    public static CalendarioAcademico Crear(
        string nombreEvento,
        string comision,
        DateTime fechaInicio,
        DateTime fechaFin,
        TipoEvento tipoEvento,
        int? materiaId = null,
        int? cursoId = null)
    {
        if (string.IsNullOrWhiteSpace(nombreEvento)) throw new ArgumentException("El nombre del evento es obligatorio.");
        if (fechaFin < fechaInicio) throw new ArgumentException("La fecha de fin no puede ser anterior a la de inicio.");

        return new CalendarioAcademico
        {
            NombreEvento = nombreEvento,
            Comision = comision,
            FechaInicio = fechaInicio.Date,
            FechaFin = fechaFin.Date,
            TipoEvento = tipoEvento,
            MateriaId = materiaId,
            CursoId = cursoId
        };
    }

    public void Modificar(string nombreEvento, string comision, DateTime fechaInicio, DateTime fechaFin, TipoEvento tipoEvento)
    {
        if (string.IsNullOrWhiteSpace(nombreEvento)) throw new ArgumentException("El nombre del evento es obligatorio.");
        if (fechaFin < fechaInicio) throw new ArgumentException("La fecha de fin no puede ser anterior a la de inicio.");
        NombreEvento = nombreEvento;
        Comision     = comision;
        FechaInicio  = fechaInicio.Date;
        FechaFin     = fechaFin.Date;
        TipoEvento   = tipoEvento;
    }
}
