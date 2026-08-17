using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Materias.DTOs;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Materias;

/// <summary>
/// CU-22: lista las materias en las que el estudiante autenticado puede inscribirse a cursar.
/// Filtra las materias del plan de su carrera que ya aprobó, en las que ya tiene una inscripción
/// activa, o cuyas correlatividades para cursar no cumple todavía.
/// </summary>
public class ListarMateriasEstudianteUseCase(
    IEstudianteRepository estudianteRepository,
    IMateriaRepository materiaRepository,
    ICorrelativiadadRepository correlativiadadRepository,
    IHistorialAcademicoRepository historialRepository,
    IInscripcionMateriaRepository inscripcionMateriaRepository)
{
    public async Task<IEnumerable<MateriaDto>> EjecutarAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el perfil de estudiante para el usuario {usuarioId}.");

        var materias = await materiaRepository.ListarPorCarreraIdAsync(estudiante.CarreraId, cancellationToken);
        var inscripcionesActivas = await inscripcionMateriaRepository.ListarActivasPorEstudianteAsync(estudiante.Id, cancellationToken);
        var materiaIdsConInscripcionActiva = inscripcionesActivas.Select(i => i.MateriaId).ToHashSet();

        var disponibles = new List<MateriaDto>();

        foreach (var materia in materias)
        {
            if (materiaIdsConInscripcionActiva.Contains(materia.Id))
                continue;

            if (await historialRepository.EstaAprobadoAsync(estudiante.Id, materia.Id, cancellationToken))
                continue;

            if (!await CumpleCorrelatividadesParaCursarAsync(estudiante.Id, materia.Id, cancellationToken))
                continue;

            disponibles.Add(materia);
        }

        return disponibles;
    }

    private async Task<bool> CumpleCorrelatividadesParaCursarAsync(
        int estudianteId,
        int materiaId,
        CancellationToken cancellationToken)
    {
        var correlatividades = await correlativiadadRepository.ObtenerParaCursarAsync(materiaId, cancellationToken);

        foreach (var correlatividad in correlatividades)
        {
            bool cumple = correlatividad.CondicionAcademica switch
            {
                CondicionAcademica.Regularizado =>
                    await historialRepository.EstaRegularizadoAsync(estudianteId, correlatividad.MateriaRequisitoId, cancellationToken),

                CondicionAcademica.Aprobado =>
                    await historialRepository.EstaAprobadoAsync(estudianteId, correlatividad.MateriaRequisitoId, cancellationToken),

                _ => false
            };

            if (!cumple)
                return false;
        }

        return true;
    }
}
