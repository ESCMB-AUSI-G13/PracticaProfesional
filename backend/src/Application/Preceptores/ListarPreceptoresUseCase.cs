using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Preceptores.DTOs;

namespace PracticaProfesional.Application.Preceptores;

public class ListarPreceptoresUseCase(IPreceptorRepository preceptorRepository)
{
    public async Task<IEnumerable<PreceptorDto>> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var preceptores = await preceptorRepository.ListarAsync(cancellationToken);
        return preceptores.Select(p => CrearPreceptorUseCase.ToDto(p, p.Usuario));
    }
}
