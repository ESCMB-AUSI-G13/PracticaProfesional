using Microsoft.Extensions.Configuration;
using PracticaProfesional.Application.Encuestas;
using PracticaProfesional.Application.Inscripciones.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Inscripciones;

/// <summary>
/// CU-22 (autogestionada): el estudiante autenticado se inscribe a una materia.
/// Bloquea si hay encuestas activas pendientes (CU-36/CU-40).
/// </summary>
public class InscribirseEnMateriaAutogestUseCase(
    IEstudianteRepository       estudianteRepository,
    InscribirseEnMateriaUseCase inscribirseUseCase,
    ObtenerEncuestaPendienteUseCase encuestaPendienteUseCase)
{
    public async Task<InscripcionMateriaResultDto> EjecutarAsync(
        int usuarioId,
        InscribirseEnMateriaAutogestDto dto,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el perfil de estudiante para el usuario {usuarioId}.");

        var pendiente = await encuestaPendienteUseCase.EjecutarAsync(estudiante.Id, cancellationToken);
        if (pendiente is not null)
            throw new BusinessException(
                "Tenés una encuesta pendiente. Completala antes de inscribirte.", 428);

        return await inscribirseUseCase.EjecutarAsync(
            new InscribirseEnMateriaDto(estudiante.Id, dto.MateriaId, dto.CursoId),
            cancellationToken);
    }
}
