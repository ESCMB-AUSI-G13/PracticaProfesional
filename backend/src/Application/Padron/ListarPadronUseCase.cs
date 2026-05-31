using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Padron.DTOs;

namespace PracticaProfesional.Application.Padron;

public class ListarPadronUseCase(IPadronRepository padronRepository)
{
    public async Task<IEnumerable<PadronAlumnoDto>> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var items = await padronRepository.ListarAsync(cancellationToken);
        return items.Select(p => new PadronAlumnoDto
        {
            DNI = p.DNI,
            FechaCarga = p.FechaCarga
        });
    }
}
