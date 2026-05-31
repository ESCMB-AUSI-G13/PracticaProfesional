using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Padron;

public class AgregarDniUseCase(IPadronRepository padronRepository)
{
    public async Task EjecutarAsync(string dni, CancellationToken cancellationToken = default)
    {
        var padron = PadronAlumno.Crear(dni);

        if (await padronRepository.ExisteDniAsync(padron.DNI, cancellationToken))
            throw new BusinessException($"El DNI {padron.DNI} ya existe en el padrón.", 409);

        await padronRepository.AgregarAsync(padron, cancellationToken);
    }
}
