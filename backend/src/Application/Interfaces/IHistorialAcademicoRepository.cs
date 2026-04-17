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
}
