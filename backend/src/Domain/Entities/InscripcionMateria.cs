using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Domain.Entities;

public class InscripcionMateria
{
    public int Id { get; private set; }
    public int EstudianteId { get; private set; }
    public Estudiante Estudiante { get; private set; } = null!;
    public int MateriaId { get; private set; }
    public Materia Materia { get; private set; } = null!;
    public int CursoId { get; private set; }
    public Curso Curso { get; private set; } = null!;
    public EstadoInscripcion Estado { get; private set; }
    public DateTime FechaInscripcion { get; private set; }

    private InscripcionMateria() { }

    public static InscripcionMateria Crear(int estudianteId, int materiaId, int cursoId)
    {
        return new InscripcionMateria
        {
            EstudianteId = estudianteId,
            MateriaId = materiaId,
            CursoId = cursoId,
            Estado = EstadoInscripcion.Activa,
            FechaInscripcion = DateTime.UtcNow
        };
    }

    public void DarDeBaja() => Estado = EstadoInscripcion.Baja;
    public void MarcarAprobada() => Estado = EstadoInscripcion.Aprobada;
    public void MarcarDesaprobada() => Estado = EstadoInscripcion.Desaprobada;
}
