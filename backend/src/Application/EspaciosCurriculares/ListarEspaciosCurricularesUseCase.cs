using PracticaProfesional.Application.EspaciosCurriculares.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.EspaciosCurriculares;

public class ListarEspaciosCurricularesUseCase(IEspacioCurricularRepository repository)
{
    public Task<IEnumerable<EspacioCurricularDto>> EjecutarAsync(CancellationToken cancellationToken = default)
        => repository.ListarAsync(cancellationToken);
}
