using PracticaProfesional.Application.Calificaciones.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Calificaciones;

/// <summary>
/// Retorna el historial completo de cambios de nota para una inscripción a examen.
/// Incluye la carga inicial (CARGAR_NOTA) y todas las rectificaciones (RECTIFICAR_NOTA),
/// ordenadas del cambio más reciente al más antiguo.
/// </summary>
public class ObtenerHistorialNotasUseCase(
    IInscripcionExamenRepository inscripcionExamenRepository,
    IAuditoriaLogRepository auditoriaLogRepository,
    IDocenteRepository docenteRepository,
    IEspacioCurricularRepository espacioCurricularRepository)
{
    public async Task<IEnumerable<CambioNotaDto>> EjecutarAsync(
        int inscripcionExamenId,
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        // Verificar que la inscripción existe
        var inscripcion = await inscripcionExamenRepository
            .ObtenerPorIdAsync(inscripcionExamenId, cancellationToken)
            ?? throw new BusinessException(
                $"No se encontró la inscripción a examen con Id {inscripcionExamenId}.");

        // Solo el docente a cargo de la materia puede ver el historial de su examen
        if (!await EsDocenteDeLaMateriaAsync(usuarioId, inscripcion.Examen.MateriaId, cancellationToken))
            throw new BusinessException("No tenés permiso para ver el historial de esta materia.", 403);

        var logs = await auditoriaLogRepository.ObtenerPorEntidadAsync(
            "InscripcionExamen",
            inscripcionExamenId.ToString(),
            cancellationToken);

        // Filtrar solo acciones de notas (CARGAR_NOTA y RECTIFICAR_NOTA)
        return logs
            .Where(l => l.Accion == "CARGAR_NOTA" || l.Accion == "RECTIFICAR_NOTA")
            .Select(l => new CambioNotaDto(
                Id:            l.Id,
                Accion:        l.Accion,
                ValorAnterior: l.ValorAnterior,
                ValorNuevo:    l.ValorNuevo,
                EjecutorEmail: l.EjecutorEmail,
                Timestamp:     l.Timestamp));
    }

    /// <summary>Mismo criterio que ListarEncuestasDocenteUseCase.EsMateriaDelDocenteAsync.</summary>
    private async Task<bool> EsDocenteDeLaMateriaAsync(int usuarioId, int materiaId, CancellationToken cancellationToken)
    {
        var docente = await docenteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
        if (docente is null) return false;

        var espacios = await espacioCurricularRepository.ListarPorDocenteIdAsync(docente.Id, cancellationToken);
        return espacios.Any(e => e.MateriaId == materiaId);
    }
}
