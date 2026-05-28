using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Preceptores.DTOs;

namespace PracticaProfesional.Application.Preceptores;

public class ListarPreceptoresUseCase(IPreceptorRepository preceptorRepository)
{
    public Task<IEnumerable<PreceptorDto>> EjecutarAsync(CancellationToken cancellationToken = default)
        => preceptorRepository.ListarAsync(cancellationToken);
}
