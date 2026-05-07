using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Interfaces;

public interface ICalendarioAcademicoRepository
{
    Task<bool> EstaEnPeriodoAsync(TipoEvento tipo, DateTime fecha, CancellationToken cancellationToken = default);
    Task<IEnumerable<CalendarioAcademico>> ListarAsync(int? anio = null, CancellationToken cancellationToken = default);
    Task<CalendarioAcademico?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task AgregarAsync(CalendarioAcademico evento, CancellationToken cancellationToken = default);
    Task EliminarAsync(CalendarioAcademico evento, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
    Task<bool> TieneEventosAsync(CancellationToken cancellationToken = default);
}
