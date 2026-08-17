using PracticaProfesional.Application.Cursos.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Cursos;

public class ListarCursosPorMateriaUseCase(ICursoRepository cursoRepository)
{
    public Task<IEnumerable<CursoDto>> EjecutarAsync(int materiaId, CancellationToken cancellationToken = default)
        => cursoRepository.ListarPorMateriaIdAsync(materiaId, cancellationToken);
}
