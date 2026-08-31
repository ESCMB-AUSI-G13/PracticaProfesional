using PracticaProfesional.Application.Examenes.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Examenes;

/// <summary>
/// Retorna los exámenes finales de las materias en las que el estudiante puede rendir:
/// las que cursa activamente, las que ya regularizó (cursada cerrada — CU-33 exige justamente
/// estar Regularizado para rendir el final) y las que tiene inscripción a examen ya cargada.
/// </summary>
public class ListarFinalesDisponiblesUseCase(
    IEstudianteRepository estudianteRepository,
    IInscripcionMateriaRepository inscripcionMateriaRepository,
    IHistorialAcademicoRepository historialRepository,
    IExamenRepository examenRepository,
    IInscripcionExamenRepository inscripcionExamenRepository)
{
    public async Task<IEnumerable<ExamenFinalDisponibleDto>> EjecutarAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new UnauthorizedAccessException("No se encontró el perfil de estudiante.");

        // Inscripciones ya existentes del estudiante — se piden antes para poder incluir sus
        // materias en el set de abajo (ver comentario más abajo).
        var misInscripciones = await inscripcionExamenRepository
            .ListarPorEstudianteAsync(estudiante.Id, cancellationToken);

        // Baja no cuenta como "ya inscripto" — mismo criterio que
        // InscripcionExamenRepository.ExisteAsync, si no la UI seguía mostrando "Inscripto"
        // (sin botón para reinscribirse) después de dar de baja.
        var yaInscriptoIds = misInscripciones
            .Where(i => i.Estado != EstadoInscripcion.Baja)
            .Select(i => i.ExamenId)
            .ToHashSet();

        // Materias activas del estudiante
        var inscripciones = await inscripcionMateriaRepository
            .ListarActivasPorEstudianteAsync(estudiante.Id, cancellationToken);

        // Materias que ya regularizó (cursada cerrada, ya no figura como "Activa")
        var historial = await historialRepository.ObtenerPorEstudianteAsync(estudiante.Id, cancellationToken);
        var regularizadas = historial
            .Where(h => h.Condicion == CondicionEstudiante.Regular || h.Condicion == CondicionEstudiante.Promocional)
            .Select(h => h.MateriaId);

        // Unión de las tres fuentes: sin esto, cerrar el curso (que pasa la inscripción de
        // Activa a Aprobada) hacía desaparecer de esta lista tanto los finales para rendir
        // como los que el estudiante ya tenía inscriptos, sin ningún lugar para darlos de baja.
        var materiaIds = inscripciones.Select(i => i.MateriaId)
            .Concat(regularizadas)
            .Concat(misInscripciones.Select(i => i.Examen.MateriaId))
            .ToHashSet();

        // Todos los finales futuros para esas materias
        var examenesDeMaterias = await examenRepository.ListarAsync(materiaIds, cancellationToken);
        var finales = examenesDeMaterias
            .Where(e =>
                e.TipoExamen == nameof(TipoExamen.Final) &&
                e.FechaExamen >= DateTime.UtcNow.Date)
            .ToList();

        if (finales.Count == 0) return [];

        // Solo se puede dar de baja mientras la inscripción sigue Activa (misma regla que
        // DarDeBajaInscripcionExamenUseCase) — por eso el Id solo se expone en ese caso.
        var inscripcionActivaPorExamen = misInscripciones
            .Where(i => i.Estado == EstadoInscripcion.Activa)
            .ToDictionary(i => i.ExamenId, i => i.Id);

        return finales.Select(e => new ExamenFinalDisponibleDto(
            e.Id,
            e.MateriaId,
            e.MateriaNombre,
            e.MateriaCodigo,
            e.FechaExamen.ToString("yyyy-MM-dd"),
            e.Horario,
            e.Cupo,
            e.TipoExamen,
            yaInscriptoIds.Contains(e.Id),
            inscripcionActivaPorExamen.TryGetValue(e.Id, out var inscripcionId) ? inscripcionId : null
        ));
    }
}
