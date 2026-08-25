using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Interfaces;

public interface ICalendarioAcademicoRepository
{
    /// <summary>
    /// Un evento sin MateriaId/CursoId es un período global (aplica a todo). Si se pasan
    /// materiaId/cursoId, también matchean eventos específicos de esa materia/curso — antes no
    /// existía forma de acotar un período, así que cualquier evento abierto habilitaba
    /// inscripción para todo el sistema (ver CHECKLIST.md, Tier 5 #19).
    /// </summary>
    Task<bool> EstaEnPeriodoAsync(
        TipoEvento tipo, DateTime fecha,
        int? materiaId = null, int? cursoId = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<CalendarioAcademico>> ListarAsync(int? anio = null, CancellationToken cancellationToken = default);
    Task<CalendarioAcademico?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task AgregarAsync(CalendarioAcademico evento, CancellationToken cancellationToken = default);
    Task EliminarAsync(CalendarioAcademico evento, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
    Task<bool> TieneEventosAsync(CancellationToken cancellationToken = default);

    /// <summary>Devuelve eventos cuya FechaFin cae dentro del rango [desde, hasta].</summary>
    Task<IEnumerable<CalendarioAcademico>> ObtenerProximosAsync(
        DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);
}
