using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface ICorrelativiadadRepository
{
    /// <summary>Retorna las correlatividades que el estudiante debe cumplir para CURSAR la materia destino.</summary>
    Task<IEnumerable<Correlatividad>> ObtenerParaCursarAsync(int materiaDestinoId, CancellationToken cancellationToken = default);

    /// <summary>Retorna las correlatividades que el estudiante debe cumplir para RENDIR el examen final.</summary>
    Task<IEnumerable<Correlatividad>> ObtenerParaRendirAsync(int materiaDestinoId, CancellationToken cancellationToken = default);

    /// <summary>Retorna todas las correlatividades de un tipo dado. Se usa para detección de ciclos.</summary>
    Task<IEnumerable<Correlatividad>> ObtenerTodasPorTipoAsync(string tipoRequerimiento, CancellationToken cancellationToken = default);

    Task AgregarAsync(Correlatividad correlatividad, CancellationToken cancellationToken = default);

    Task<IEnumerable<Correlatividad>> ListarPorMateriaDestinoAsync(int materiaDestinoId, CancellationToken cancellationToken = default);
    Task<Correlatividad?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task EliminarAsync(Correlatividad correlatividad, CancellationToken cancellationToken = default);
}
