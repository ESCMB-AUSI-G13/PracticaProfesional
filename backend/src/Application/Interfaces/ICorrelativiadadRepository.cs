using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface ICorrelativiadadRepository
{
    /// <summary>Retorna las correlatividades que el estudiante debe cumplir para CURSAR la materia destino.</summary>
    Task<IEnumerable<Correlatividad>> ObtenerParaCursarAsync(int materiaDestinoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Igual que <see cref="ObtenerParaCursarAsync"/> pero para varias materias destino en un solo
    /// viaje a la base — evita N+1 al listar disponibilidad de un plan completo.
    /// </summary>
    Task<IEnumerable<Correlatividad>> ObtenerParaCursarPorMateriasAsync(IEnumerable<int> materiaDestinoIds, CancellationToken cancellationToken = default);

    /// <summary>Retorna las correlatividades que el estudiante debe cumplir para RENDIR el examen final.</summary>
    Task<IEnumerable<Correlatividad>> ObtenerParaRendirAsync(int materiaDestinoId, CancellationToken cancellationToken = default);

    /// <summary>Retorna todas las correlatividades de un tipo dado. Se usa para detección de ciclos.</summary>
    Task<IEnumerable<Correlatividad>> ObtenerTodasPorTipoAsync(string tipoRequerimiento, CancellationToken cancellationToken = default);

    Task AgregarAsync(Correlatividad correlatividad, CancellationToken cancellationToken = default);

    Task<IEnumerable<Correlatividad>> ListarPorMateriaDestinoAsync(int materiaDestinoId, CancellationToken cancellationToken = default);
    Task<Correlatividad?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task EliminarAsync(Correlatividad correlatividad, CancellationToken cancellationToken = default);
}
