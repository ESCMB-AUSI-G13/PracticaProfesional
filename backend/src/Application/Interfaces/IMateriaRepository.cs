namespace PracticaProfesional.Application.Interfaces;

/// <summary>
/// Repositorio de materias. Proporciona consultas sobre el plan académico.
/// </summary>
public interface IMateriaRepository
{
    /// <summary>
    /// Devuelve el identificador del plan académico al que pertenece la materia.
    /// Retorna <c>null</c> si la materia no existe.
    /// </summary>
    Task<string?> ObtenerPlanAsync(int materiaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve la cantidad total de materias que conforman el plan académico indicado.
    /// </summary>
    Task<int> ContarPorPlanAsync(string plan, CancellationToken cancellationToken = default);
}
