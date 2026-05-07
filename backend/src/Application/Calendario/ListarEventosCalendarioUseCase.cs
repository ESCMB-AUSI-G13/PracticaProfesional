using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Calendario;

public class ListarEventosCalendarioUseCase(ICalendarioAcademicoRepository calendarioRepository)
{
    public async Task<IEnumerable<EventoCalendarioDto>> EjecutarAsync(
        int? anio = null, CancellationToken cancellationToken = default)
    {
        var eventos = await calendarioRepository.ListarAsync(anio, cancellationToken);
        return eventos.Select(e => new EventoCalendarioDto(
            e.Id,
            e.NombreEvento,
            e.Comision,
            e.FechaInicio,
            e.FechaFin,
            e.TipoEvento.ToString()));
    }
}
