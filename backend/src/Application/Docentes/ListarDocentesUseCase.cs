using PracticaProfesional.Application.Docentes.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Docentes;

public class ListarDocentesUseCase(IDocenteRepository docenteRepository)
{
    public Task<IEnumerable<DocenteDto>> EjecutarAsync(CancellationToken cancellationToken = default)
        => docenteRepository.ListarAsync(cancellationToken);
}
