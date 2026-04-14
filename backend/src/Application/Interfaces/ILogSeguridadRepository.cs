using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface ILogSeguridadRepository
{
    Task AgregarAsync(LogSeguridad log, CancellationToken cancellationToken = default);

    Task<(IEnumerable<LogSeguridad> Items, int Total)> ListarAsync(
        string? email,
        bool? soloFallidos,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int pagina,
        int tamanoPagina,
        CancellationToken cancellationToken = default);
}
