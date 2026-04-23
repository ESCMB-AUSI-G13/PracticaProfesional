using PracticaProfesional.Application.Examenes.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Examenes;

public class ListarExamenesUseCase(IExamenRepository examenRepository)
{
    public async Task<IEnumerable<ExamenDto>> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var examenes = await examenRepository.ListarAsync(cancellationToken);
        return examenes.Select(e => CrearExamenUseCase.ToDto(e, e.Materia.Nombre));
    }
}
