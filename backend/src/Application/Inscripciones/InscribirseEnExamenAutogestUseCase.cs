using PracticaProfesional.Application.Encuestas;
using PracticaProfesional.Application.Inscripciones.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Inscripciones;

/// <summary>
/// CU-33 (autogestionada): el estudiante autenticado se inscribe a un examen final.
/// Bloquea si hay encuestas activas pendientes (CU-36/CU-40). Mismo patrón que
/// InscribirseEnMateriaAutogestUseCase: resuelve el Estudiante desde el usuario del token y
/// delega la inscripción propiamente dicha al UseCase administrativo.
/// </summary>
public class InscribirseEnExamenAutogestUseCase(
    IEstudianteRepository           estudianteRepository,
    InscribirseEnExamenUseCase      inscribirseUseCase,
    ObtenerEncuestaPendienteUseCase encuestaPendienteUseCase)
{
    public async Task<InscripcionExamenResultDto> EjecutarAsync(
        int usuarioId,
        int examenId,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el perfil de estudiante para el usuario {usuarioId}.");

        var pendiente = await encuestaPendienteUseCase.EjecutarAsync(estudiante.Id, cancellationToken);
        if (pendiente is not null)
            throw new BusinessException(
                "Tenés una encuesta pendiente. Completala antes de inscribirte.", 428);

        return await inscribirseUseCase.EjecutarAsync(
            new InscribirseEnExamenDto(estudiante.Id, examenId),
            cancellationToken);
    }
}
