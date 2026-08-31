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

    /// <summary>
    /// Trae todo el historial del estudiante en un solo viaje a la base, para evaluar
    /// EstaRegularizado/EstaAprobado en memoria contra varias materias sin repetir consultas
    /// (evita N+1 al listar disponibilidad de un plan completo).
    /// </summary>
    Task<IEnumerable<HistorialAcademico>> ObtenerPorEstudianteAsync(int estudianteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Igual que <see cref="ObtenerPorEstudianteAsync"/> pero con la navegación a Materia
    /// cargada — para vistas que necesitan mostrar código/nombre (CU-43, "mi historial").
    /// </summary>
    Task<IEnumerable<HistorialAcademico>> ObtenerConMateriaPorEstudianteAsync(int estudianteId, CancellationToken cancellationToken = default);

    Task<decimal?> ObtenerNotaFinalEnCursoAsync(
        int estudianteId,
        int materiaId,
        int cursoId,
        CancellationToken cancellationToken = default);

    Task<int> ContarAprobadosEnCarreraAsync(
        int estudianteId,
        int carreraId,
        CancellationToken cancellationToken = default);
    Task<bool> ExistePorMateriaIdAsync(int materiaId, CancellationToken cancellationToken = default);

    Task AgregarAsync(HistorialAcademico historial, CancellationToken cancellationToken = default);
    Task AgregarRangoAsync(IEnumerable<HistorialAcademico> historiales, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
