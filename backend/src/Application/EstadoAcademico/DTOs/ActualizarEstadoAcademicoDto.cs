namespace PracticaProfesional.Application.EstadoAcademico.DTOs;

/// <summary>
/// Parámetros de entrada para evaluar y actualizar el estado académico de un estudiante.
/// </summary>
public class ActualizarEstadoAcademicoDto
{
    /// <summary>Id del estudiante a evaluar.</summary>
    public int EstudianteId { get; set; }

    /// <summary>
    /// Id de la materia que disparó la evaluación (opcional).
    /// Si se provee junto con <see cref="CursoId"/>, se evalúan los criterios
    /// de asistencia, nota final y egreso para esa cursada específica.
    /// </summary>
    public int? MateriaId { get; set; }

    /// <summary>
    /// Id del curso que disparó la evaluación (opcional).
    /// Requerido junto con <see cref="MateriaId"/> para evaluación por cursada.
    /// </summary>
    public int? CursoId { get; set; }
}
