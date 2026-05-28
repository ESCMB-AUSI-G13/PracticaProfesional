using PracticaProfesional.Application.Estudiantes.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Estudiantes;

public class ListarEstudiantesUseCase(IEstudianteRepository estudianteRepository)
{
    public Task<IEnumerable<EstudianteDto>> EjecutarAsync(CancellationToken cancellationToken = default)
        => estudianteRepository.ListarAsync(cancellationToken);
}
