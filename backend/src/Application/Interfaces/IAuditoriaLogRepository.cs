using PracticaProfesional.Application.Auditoria.DTOs;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IAuditoriaLogRepository
{
    Task AgregarAsync(AuditoriaLog log, CancellationToken cancellationToken = default);

    Task<(IEnumerable<AuditoriaLog> Items, int Total)> ListarAsync(
        AuditoriaLogFiltroDto filtro,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna todos los registros de auditoría para una entidad específica,
    /// ordenados del más reciente al más antiguo. Usado para historial de cambios.
    /// </summary>
    Task<IEnumerable<AuditoriaLog>> ObtenerPorEntidadAsync(
        string entidadTipo,
        string entidadId,
        CancellationToken cancellationToken = default);
}
