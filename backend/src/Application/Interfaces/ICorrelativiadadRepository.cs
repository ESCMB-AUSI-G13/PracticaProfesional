using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface ICorrelativiadadRepository
{
    /// <summary>Retorna las correlatividades que el estudiante debe cumplir para CURSAR la materia destino.</summary>
    Task<IEnumerable<Correlatividad>> ObtenerParaCursarAsync(int materiaDestinoId, CancellationToken cancellationToken = default);

    /// <summary>Retorna las correlatividades que el estudiante debe cumplir para RENDIR el examen final.</summary>
    Task<IEnumerable<Correlatividad>> ObtenerParaRendirAsync(int materiaDestinoId, CancellationToken cancellationToken = default);
}
