using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IHistorialAcademicoRepository
{
    /// <summary>Retorna todos los registros de historial de un estudiante para una materia dada.</summary>
    Task<IEnumerable<HistorialAcademico>> ObtenerPorEstudianteYMateriaAsync(
        int estudianteId,
        int materiaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Indica si el estudiante tiene condición Regularizado en la materia.
    ///     Condición Regularizado = existe un historial con Condicion Regular o Promocional.
    /// </summary>
    Task<bool> EstaRegularizadoAsync(int estudianteId, int materiaId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Indica si el estudiante aprobó la materia (NotaFinal >= 4).
    /// </summary>
    Task<bool> EstaAprobadoAsync(int estudianteId, int materiaId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Devuelve la nota final obtenida por el estudiante en una materia dentro de un curso concreto.
    ///     Retorna <c>null</c> si aún no hay nota registrada para esa cursada.
    /// </summary>
    Task<decimal?> ObtenerNotaFinalEnCursoAsync(
        int estudianteId,
        int materiaId,
        int cursoId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Devuelve la cantidad de materias distintas que el estudiante tiene aprobadas
    ///     (NotaFinal >= 4) dentro del plan académico indicado.
    /// </summary>
    Task<int> ContarAprobadosEnPlanAsync(
        int estudianteId,
        string plan,
        CancellationToken cancellationToken = default);
}
