using PracticaProfesional.Application.EspaciosCurriculares.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.EspaciosCurriculares;

public class ListarEspaciosCurricularesUseCase(IEspacioCurricularRepository repository)
{
    public async Task<IEnumerable<EspacioCurricularDto>> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var lista = await repository.ListarAsync(cancellationToken);
        return lista.Select(ec => CrearEspacioCurricularUseCase.ToDto(ec, ec.Materia, ec.Docente, ec.Curso));
    }
}
