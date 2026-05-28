using PracticaProfesional.Application.Cursos.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Cursos;

public class ListarCursosUseCase(ICursoRepository cursoRepository)
{
    public Task<IEnumerable<CursoDto>> EjecutarAsync(CancellationToken cancellationToken = default)
        => cursoRepository.ListarAsync(cancellationToken);
}
