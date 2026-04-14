using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Domain.Entities;

public class Asistencia
{
    public int Id { get; private set; }
    public int EstudianteId { get; private set; }
    public Estudiante Estudiante { get; private set; } = null!;
    public int MateriaId { get; private set; }
    public Materia Materia { get; private set; } = null!;
    public int CursoId { get; private set; }
    public Curso Curso { get; private set; } = null!;
    public DateTime Fecha { get; private set; }
    public EstadoAsistencia Estado { get; private set; }
    public string? Motivo { get; private set; }

    private Asistencia() { }

    public static Asistencia Registrar(
        int estudianteId,
        int materiaId,
        int cursoId,
        DateTime fecha,
        EstadoAsistencia estado,
        string? motivo = null)
    {
        if (estado == EstadoAsistencia.AusenteJustificado && string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("El motivo es obligatorio para ausencia justificada.");

        return new Asistencia
        {
            EstudianteId = estudianteId,
            MateriaId = materiaId,
            CursoId = cursoId,
            Fecha = fecha.Date,
            Estado = estado,
            Motivo = motivo
        };
    }

    public void Rectificar(EstadoAsistencia nuevoEstado, string? motivo)
    {
        if (nuevoEstado == EstadoAsistencia.AusenteJustificado && string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("El motivo es obligatorio para ausencia justificada.");
        Estado = nuevoEstado;
        Motivo = motivo;
    }
}
