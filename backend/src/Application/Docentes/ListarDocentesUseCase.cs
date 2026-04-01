using PracticaProfesional.Application.Docentes.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Docentes;

public class ListarDocentesUseCase(IDocenteRepository docenteRepository)
{
    public async Task<IEnumerable<DocenteDto>> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var docentes = await docenteRepository.ListarAsync(cancellationToken);
        return docentes.Select(d => CrearDocenteUseCase.ToDto(d, d.Usuario));
    }
}
