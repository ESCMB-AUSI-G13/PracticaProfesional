using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IHistorialAcademicoRepository
{
    Task<IEnumerable<HistorialAcademico>> ObtenerPorEstudianteYMateriaAsync(
        int estudianteId,
        int materiaId,
        CancellationToken cancellationToken = default);

    Task<bool> EstaRegularizadoAsync(int estudianteId, int materiaId, CancellationToken cancellationToken = default);

    Task<bool> EstaAprobadoAsync(int estudianteId, int materiaId, CancellationToken cancellationToken = default);

    Task<decimal?> ObtenerNotaFinalEnCursoAsync(
        int estudianteId,
        int materiaId,
        int cursoId,
        CancellationToken cancellationToken = default);

    Task<int> ContarAprobadosEnCarreraAsync(
        int estudianteId,
        int carreraId,
        CancellationToken cancellationToken = default);
}
