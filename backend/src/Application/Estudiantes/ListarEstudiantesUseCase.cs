using PracticaProfesional.Application.Estudiantes.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Estudiantes;

public class ListarEstudiantesUseCase(IEstudianteRepository estudianteRepository)
{
    public async Task<IEnumerable<EstudianteDto>> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var estudiantes = await estudianteRepository.ListarAsync(cancellationToken);
        return estudiantes.Select(e => CrearEstudianteUseCase.ToDto(e, e.Usuario));
    }
}
