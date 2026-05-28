using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Materias.DTOs;

namespace PracticaProfesional.Application.Materias;

public class ListarMateriasUseCase(IMateriaRepository materiaRepository)
{
    public Task<IEnumerable<MateriaDto>> EjecutarAsync(CancellationToken cancellationToken = default)
        => materiaRepository.ListarAsync(cancellationToken);
}
