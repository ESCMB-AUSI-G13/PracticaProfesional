using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Calendario;

public class CrearEventoCalendarioUseCase(ICalendarioAcademicoRepository calendarioRepository)
{
    public async Task<EventoCalendarioDto> EjecutarAsync(
        CrearEventoCalendarioDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.NombreEvento))
            throw new BusinessException("El nombre del evento es obligatorio.");
        if (dto.FechaFin < dto.FechaInicio)
            throw new BusinessException("La fecha de fin no puede ser anterior a la de inicio.");

        var evento = CalendarioAcademico.Crear(
            dto.NombreEvento,
            dto.Comision ?? string.Empty,
            dto.FechaInicio,
            dto.FechaFin,
            dto.TipoEvento);

        await calendarioRepository.AgregarAsync(evento, cancellationToken);

        return new EventoCalendarioDto(
            evento.Id,
            evento.NombreEvento,
            evento.Comision,
            evento.FechaInicio,
            evento.FechaFin,
            evento.TipoEvento.ToString());
    }
}
