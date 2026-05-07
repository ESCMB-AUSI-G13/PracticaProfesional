using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Correlatividades;

public class EliminarCorrelativiadadUseCase(ICorrelativiadadRepository correlativiadadRepository)
{
    public async Task EjecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var correlatividad = await correlativiadadRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró la correlatividad con Id {id}.");

        await correlativiadadRepository.EliminarAsync(correlatividad, cancellationToken);
    }
}
