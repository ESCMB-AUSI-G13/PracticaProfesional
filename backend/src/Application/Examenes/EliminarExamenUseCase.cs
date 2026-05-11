using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Examenes;

public class EliminarExamenUseCase(IExamenRepository examenRepository)
{
    public Task EjecutarAsync(int id, CancellationToken cancellationToken = default)
        => examenRepository.EliminarAsync(id, cancellationToken);
}
