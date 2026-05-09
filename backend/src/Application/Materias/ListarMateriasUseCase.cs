using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Materias.DTOs;

namespace PracticaProfesional.Application.Materias;

public class ListarMateriasUseCase(IMateriaRepository materiaRepository)
{
    public async Task<IEnumerable<MateriaDto>> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var materias = await materiaRepository.ListarAsync(cancellationToken);
        return materias.Select(CrearMateriaUseCase.ToDtoConNavegacion);
    }
}
