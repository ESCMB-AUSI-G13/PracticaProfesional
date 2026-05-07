using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Calendario;

public class EliminarEventoCalendarioUseCase(ICalendarioAcademicoRepository calendarioRepository)
{
    public async Task EjecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var evento = await calendarioRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró el evento con Id {id}.");

        await calendarioRepository.EliminarAsync(evento, cancellationToken);
    }
}
