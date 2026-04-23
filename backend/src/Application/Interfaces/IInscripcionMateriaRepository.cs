using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IInscripcionMateriaRepository
{
    Task<IEnumerable<InscripcionMateria>> ListarAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<InscripcionMateria>> ListarActivasPorMateriaAsync(int materiaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InscripcionMateria>> ListarActivasPorEstudianteAsync(int estudianteId, CancellationToken cancellationToken = default);
    Task<bool> ExisteInscripcionActivaAsync(int estudianteId, int materiaId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Indica si el estudiante tiene al menos una inscripción activa en cualquier materia.
    ///     Se usa para detectar abandono académico.
    /// </summary>
    Task<bool> TieneAlgunaInscripcionActivaAsync(int estudianteId, CancellationToken cancellationToken = default);
    Task<InscripcionMateria?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task AgregarAsync(InscripcionMateria inscripcion, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
