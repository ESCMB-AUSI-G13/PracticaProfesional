using PracticaProfesional.Application.Cursos.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Cursos;

public class ListarCursosUseCase(ICursoRepository cursoRepository)
{
    public async Task<IEnumerable<CursoDto>> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var cursos = await cursoRepository.ListarAsync(cancellationToken);
        return cursos.Select(c => CrearCursoUseCase.ToDto(c, c.Preceptor));
    }
}
