using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Padron;

public class EliminarDniUseCase(IPadronRepository padronRepository)
{
    public async Task EjecutarAsync(string dni, CancellationToken cancellationToken = default)
    {
        var eliminado = await padronRepository.EliminarAsync(dni.Trim(), cancellationToken);
        if (!eliminado)
            throw new BusinessException($"El DNI {dni} no existe en el padrón.", 404);
    }
}
