using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Materias.DTOs;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Materias;

public class ListarMateriasEstudianteUseCase(
    IEstudianteRepository estudianteRepository,
    IMateriaRepository materiaRepository)
{
    public async Task<IEnumerable<MateriaDto>> EjecutarAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el perfil de estudiante para el usuario {usuarioId}.");

        var materias = await materiaRepository.ListarPorCarreraIdAsync(estudiante.CarreraId, cancellationToken);
        return materias.Select(CrearMateriaUseCase.ToDtoConNavegacion);
    }
}
