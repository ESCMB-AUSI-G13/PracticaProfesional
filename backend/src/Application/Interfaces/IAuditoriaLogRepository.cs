using PracticaProfesional.Application.Auditoria.DTOs;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IAuditoriaLogRepository
{
    Task AgregarAsync(AuditoriaLog log, CancellationToken cancellationToken = default);

    Task<(IEnumerable<AuditoriaLog> Items, int Total)> ListarAsync(
        AuditoriaLogFiltroDto filtro,
        CancellationToken cancellationToken = default);
}
