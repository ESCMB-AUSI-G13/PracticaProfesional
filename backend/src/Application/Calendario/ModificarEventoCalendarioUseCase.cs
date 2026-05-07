using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Calendario;

public class ModificarEventoCalendarioUseCase(ICalendarioAcademicoRepository calendarioRepository)
{
    public async Task<EventoCalendarioDto> EjecutarAsync(
        int id, ModificarEventoCalendarioDto dto, CancellationToken cancellationToken = default)
    {
        var evento = await calendarioRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró el evento con Id {id}.");

        evento.Modificar(dto.NombreEvento, dto.Comision ?? string.Empty, dto.FechaInicio, dto.FechaFin, dto.TipoEvento);
        await calendarioRepository.GuardarCambiosAsync(cancellationToken);

        return new EventoCalendarioDto(
            evento.Id,
            evento.NombreEvento,
            evento.Comision,
            evento.FechaInicio,
            evento.FechaFin,
            evento.TipoEvento.ToString());
    }
}
