using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Domain.Entities;

public class HistorialAcademico
{
    public int Id { get; private set; }
    public int EstudianteId { get; private set; }
    public Estudiante Estudiante { get; private set; } = null!;
    public int MateriaId { get; private set; }
    public Materia Materia { get; private set; } = null!;
    public int CursoId { get; private set; }
    public Curso Curso { get; private set; } = null!;
    public int Anio { get; private set; }
    public string Comision { get; private set; } = string.Empty;
    public string EstadoFinal { get; private set; } = string.Empty;
    public decimal? NotaFinal { get; private set; }
    public CondicionEstudiante Condicion { get; private set; }

    private HistorialAcademico() { }

    public static HistorialAcademico Crear(
        int estudianteId,
        int materiaId,
        int cursoId,
        int anio,
        string comision,
        string estadoFinal,
        decimal? notaFinal,
        CondicionEstudiante condicion)
    {
        if (string.IsNullOrWhiteSpace(comision)) throw new ArgumentException("La comisión es obligatoria.");
        if (string.IsNullOrWhiteSpace(estadoFinal)) throw new ArgumentException("El estado final es obligatorio.");

        return new HistorialAcademico
        {
            EstudianteId = estudianteId,
            MateriaId = materiaId,
            CursoId = cursoId,
            Anio = anio,
            Comision = comision,
            EstadoFinal = estadoFinal,
            NotaFinal = notaFinal,
            Condicion = condicion
        };
    }
}
