using PracticaProfesional.Application.Carreras.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Carreras;

public class ListarCarrerasUseCase(ICarreraRepository carreraRepository)
{
    public async Task<IEnumerable<CarreraDto>> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var carreras = await carreraRepository.ListarAsync(cancellationToken);
        return carreras.Select(c => new CarreraDto(c.Id, c.Nombre, c.Resolucion));
    }
}
