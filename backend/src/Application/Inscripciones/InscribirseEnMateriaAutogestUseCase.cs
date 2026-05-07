using PracticaProfesional.Application.Inscripciones.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Inscripciones;

/// <summary>
/// CU-22 (autogestionada): el estudiante autenticado se inscribe a una materia.
/// Resuelve el EstudianteId desde el UsuarioId del JWT y delega al UseCase base.
/// </summary>
public class InscribirseEnMateriaAutogestUseCase(
    IEstudianteRepository estudianteRepository,
    InscribirseEnMateriaUseCase inscribirseUseCase)
{
    public async Task<InscripcionMateriaResultDto> EjecutarAsync(
        int usuarioId,
        InscribirseEnMateriaAutogestDto dto,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el perfil de estudiante para el usuario {usuarioId}.");

        return await inscribirseUseCase.EjecutarAsync(
            new InscribirseEnMateriaDto(estudiante.Id, dto.MateriaId, dto.CursoId),
            cancellationToken);
    }
}
