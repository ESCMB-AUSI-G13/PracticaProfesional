using PracticaProfesional.Application.Examenes.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Examenes;

public class ListarExamenesUseCase(IExamenRepository examenRepository)
{
    public Task<IEnumerable<ExamenDto>> EjecutarAsync(CancellationToken cancellationToken = default)
        => examenRepository.ListarAsync(cancellationToken);
}
