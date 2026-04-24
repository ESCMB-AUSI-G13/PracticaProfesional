using PracticaProfesional.Application.Examenes.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Examenes;

/// <summary>
/// Retorna los exámenes finales de las materias en las que el estudiante
/// tiene una inscripción activa, indicando si ya está inscripto.
/// </summary>
public class ListarFinalesDisponiblesUseCase(
    IEstudianteRepository estudianteRepository,
    IInscripcionMateriaRepository inscripcionMateriaRepository,
    IExamenRepository examenRepository,
    IInscripcionExamenRepository inscripcionExamenRepository)
{
    public async Task<IEnumerable<ExamenFinalDisponibleDto>> EjecutarAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new UnauthorizedAccessException("No se encontró el perfil de estudiante.");

        // Materias activas del estudiante
        var inscripciones = await inscripcionMateriaRepository
            .ListarActivasPorEstudianteAsync(estudiante.Id, cancellationToken);

        var materiaIds = inscripciones.Select(i => i.MateriaId).Distinct().ToHashSet();

        // Todos los finales futuros para esas materias
        var todosExamenes = await examenRepository.ListarAsync(cancellationToken);
        var finales = todosExamenes
            .Where(e =>
                materiaIds.Contains(e.MateriaId) &&
                e.TipoExamen == TipoExamen.Final &&
                e.FechaExamen >= DateTime.UtcNow.Date)
            .ToList();

        if (finales.Count == 0) return [];

        // Inscripciones ya existentes del estudiante
        var misInscripciones = await inscripcionExamenRepository
            .ListarPorEstudianteAsync(estudiante.Id, cancellationToken);
        var yaInscriptoIds = misInscripciones.Select(i => i.ExamenId).ToHashSet();

        return finales.Select(e => new ExamenFinalDisponibleDto(
            e.Id,
            e.MateriaId,
            e.Materia?.Nombre ?? string.Empty,
            e.Materia?.Codigo ?? string.Empty,
            e.FechaExamen.ToString("yyyy-MM-dd"),
            e.Horario,
            e.Cupo,
            e.TipoExamen.ToString(),
            yaInscriptoIds.Contains(e.Id)
        ));
    }
}
