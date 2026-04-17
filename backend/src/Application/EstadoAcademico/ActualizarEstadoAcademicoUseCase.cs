using PracticaProfesional.Application.EstadoAcademico.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.EstadoAcademico;

/// <summary>
/// CU-43: Actualización automática del estado académico del estudiante.
///
/// Evalúa, en orden de prioridad, los siguientes criterios:
///   1. Egreso   — todas las materias del plan aprobadas (NotaFinal ≥ 4).
///   2. Pérdida de regularidad → Libre:
///        a) Ausencias injustificadas superan el 25 % de las clases del curso, o
///        b) Nota final de cursada es reprobatoria (NotaFinal &lt; 4).
///   3. Promoción → Promocional:
///        NotaFinal ≥ 7 y asistencia efectiva ≥ 75 % (solo desde estado Regular).
///   4. Deserción — sin actividad académica durante más de 2 años y sin inscripciones activas.
///
/// La evaluación de criterios 1–3 requiere que se indiquen MateriaId y CursoId.
/// El criterio de deserción (4) se evalúa siempre.
/// </summary>
public class ActualizarEstadoAcademicoUseCase(
    IEstudianteRepository estudianteRepository,
    IHistorialAcademicoRepository historialRepository,
    IAsistenciaRepository asistenciaRepository,
    IMateriaRepository materiaRepository,
    IInscripcionMateriaRepository inscripcionMateriaRepository,
    IAuditoriaService auditoria)
{
    // ── Umbrales de negocio (configurables) ─────────────────────────────────────
    private const decimal NotaMinPromocion          = 7m;
    private const decimal AsistenciaMinPromocion    = 0.75m;  // 75 % de presencia efectiva
    private const decimal AusenciaMaxRegularidad    = 0.25m;  // 25 % de ausencias injustificadas
    private const int     AniosInactividadDesercion = 2;

    public async Task<ResultadoActualizacionEstadoDto> EjecutarAsync(
        ActualizarEstadoAcademicoDto dto,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await estudianteRepository.ObtenerPorIdAsync(dto.EstudianteId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el estudiante con Id {dto.EstudianteId}.");

        // Estado terminal: no se reevalúa
        if (estudiante.Condicion == CondicionEstudiante.Egresado)
            return BuildResultado(estudiante.Id, estudiante.Condicion, estudiante.Condicion,
                false, "El estudiante ya se encuentra en estado Egresado.");

        var condicionAnterior = estudiante.Condicion;

        // ── Criterios que requieren materia y curso ──────────────────────────────
        if (dto.MateriaId.HasValue && dto.CursoId.HasValue)
        {
            int materiaId = dto.MateriaId.Value;
            int cursoId   = dto.CursoId.Value;

            // ── 1. EGRESO ────────────────────────────────────────────────────────
            var motivo = await EvaluarEgresoAsync(dto.EstudianteId, materiaId, cancellationToken);
            if (motivo is not null)
            {
                estudiante.Egresar();
                return await GuardarYAuditarAsync(estudiante, condicionAnterior, motivo, cancellationToken);
            }

            // ── 2. PÉRDIDA DE REGULARIDAD (por asistencia o nota reprobatoria) ───
            motivo = await EvaluarPerdidaRegularidadAsync(
                dto.EstudianteId, materiaId, cursoId, estudiante.Condicion, cancellationToken);
            if (motivo is not null)
            {
                estudiante.PerderRegularidad();
                return await GuardarYAuditarAsync(estudiante, condicionAnterior, motivo, cancellationToken);
            }

            // ── 3. PROMOCIÓN ─────────────────────────────────────────────────────
            motivo = await EvaluarPromocionAsync(
                dto.EstudianteId, materiaId, cursoId, estudiante.Condicion, cancellationToken);
            if (motivo is not null)
            {
                estudiante.ObtenerPromocion();
                return await GuardarYAuditarAsync(estudiante, condicionAnterior, motivo, cancellationToken);
            }
        }

        // ── 4. DESERCIÓN (evaluación general, siempre) ──────────────────────────
        if (estudiante.Condicion != CondicionEstudiante.Desertor)
        {
            var motivoDesercion = await EvaluarDesercionAsync(dto.EstudianteId, cancellationToken);
            if (motivoDesercion is not null)
            {
                estudiante.Desertar();
                return await GuardarYAuditarAsync(estudiante, condicionAnterior, motivoDesercion, cancellationToken);
            }
        }

        return BuildResultado(estudiante.Id, condicionAnterior, estudiante.Condicion,
            false, "Sin cambios: ningún criterio de transición fue superado.");
    }

    // ── Evaluadores privados ─────────────────────────────────────────────────────

    /// <summary>
    /// Retorna el motivo de egreso si el estudiante aprobó todas las materias de su plan;
    /// de lo contrario retorna null.
    /// </summary>
    private async Task<string?> EvaluarEgresoAsync(
        int estudianteId, int materiaId, CancellationToken ct)
    {
        var plan = await materiaRepository.ObtenerPlanAsync(materiaId, ct);
        if (plan is null) return null;

        var totalPlan  = await materiaRepository.ContarPorPlanAsync(plan, ct);
        if (totalPlan == 0) return null;

        var aprobados  = await historialRepository.ContarAprobadosEnPlanAsync(estudianteId, plan, ct);
        if (aprobados >= totalPlan)
            return $"Aprobación total del plan académico '{plan}' ({aprobados}/{totalPlan} materias)";

        return null;
    }

    /// <summary>
    /// Retorna el motivo de pérdida de regularidad si se supera el umbral de ausencias
    /// o la nota final es reprobatoria; de lo contrario retorna null.
    /// Solo aplica si el estudiante aún no es Libre.
    /// </summary>
    private async Task<string?> EvaluarPerdidaRegularidadAsync(
        int estudianteId, int materiaId, int cursoId,
        CondicionEstudiante condicion, CancellationToken ct)
    {
        if (condicion == CondicionEstudiante.Libre) return null;

        // a) Ausencias injustificadas > 25 %
        var (total, ausentesInjust, _) =
            await asistenciaRepository.ObtenerEstadisticasAsync(estudianteId, materiaId, cursoId, ct);

        if (total > 0)
        {
            decimal tasaAusencia = (decimal)ausentesInjust / total;
            if (tasaAusencia > AusenciaMaxRegularidad)
                return $"Ausencias injustificadas ({tasaAusencia:P1}) superan el umbral del {AusenciaMaxRegularidad:P0}";
        }

        // b) Nota final reprobatoria
        var notaFinal = await historialRepository.ObtenerNotaFinalEnCursoAsync(
            estudianteId, materiaId, cursoId, ct);

        if (notaFinal.HasValue && notaFinal.Value < 4m)
            return $"Nota final reprobatoria ({notaFinal.Value:F2}) en la cursada";

        return null;
    }

    /// <summary>
    /// Retorna el motivo de promoción si la nota final ≥ 7 y la asistencia efectiva ≥ 75 %;
    /// de lo contrario retorna null. Solo aplica si el estudiante está en estado Regular.
    /// </summary>
    private async Task<string?> EvaluarPromocionAsync(
        int estudianteId, int materiaId, int cursoId,
        CondicionEstudiante condicion, CancellationToken ct)
    {
        if (condicion != CondicionEstudiante.Regular) return null;

        var notaFinal = await historialRepository.ObtenerNotaFinalEnCursoAsync(
            estudianteId, materiaId, cursoId, ct);
        if (!notaFinal.HasValue || notaFinal.Value < NotaMinPromocion) return null;

        var (total, ausentesInjust, _) =
            await asistenciaRepository.ObtenerEstadisticasAsync(estudianteId, materiaId, cursoId, ct);

        decimal tasaPresencia = total > 0
            ? (decimal)(total - ausentesInjust) / total
            : 0m;

        if (tasaPresencia >= AsistenciaMinPromocion)
            return $"Nota final {notaFinal.Value:F2} ≥ {NotaMinPromocion} " +
                   $"y asistencia efectiva {tasaPresencia:P1} ≥ {AsistenciaMinPromocion:P0}";

        return null;
    }

    /// <summary>
    /// Retorna el motivo de deserción si el estudiante no tiene inscripciones activas
    /// y su última actividad supera los años de inactividad permitidos;
    /// de lo contrario retorna null.
    /// </summary>
    private async Task<string?> EvaluarDesercionAsync(int estudianteId, CancellationToken ct)
    {
        var tieneInscripcionActiva =
            await inscripcionMateriaRepository.TieneAlgunaInscripcionActivaAsync(estudianteId, ct);
        if (tieneInscripcionActiva) return null;

        var ultimaActividad = await asistenciaRepository.ObtenerUltimaFechaActividadAsync(estudianteId, ct);
        var fechaLimite     = DateTime.UtcNow.AddYears(-AniosInactividadDesercion);

        if (ultimaActividad is null)
            return "Sin actividad académica registrada y sin inscripciones activas";

        if (ultimaActividad.Value < fechaLimite)
            return $"Sin actividad académica desde {ultimaActividad.Value:yyyy-MM-dd} " +
                   $"(más de {AniosInactividadDesercion} años de inactividad)";

        return null;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private async Task<ResultadoActualizacionEstadoDto> GuardarYAuditarAsync(
        Domain.Entities.Estudiante estudiante,
        CondicionEstudiante condicionAnterior,
        string motivo,
        CancellationToken ct)
    {
        await estudianteRepository.GuardarCambiosAsync(ct);

        await auditoria.RegistrarAsync(
            "Estudiante",
            estudiante.Id.ToString(),
            "ESTADO_AUTOMATICO",
            valorAnterior: condicionAnterior.ToString(),
            valorNuevo: new { Condicion = estudiante.Condicion.ToString(), Motivo = motivo },
            ct);

        return BuildResultado(estudiante.Id, condicionAnterior, estudiante.Condicion, true, motivo);
    }

    private static ResultadoActualizacionEstadoDto BuildResultado(
        int estudianteId,
        CondicionEstudiante anterior,
        CondicionEstudiante nueva,
        bool huboTransicion,
        string motivo) =>
        new()
        {
            EstudianteId      = estudianteId,
            CondicionAnterior = anterior.ToString(),
            CondicionNueva    = nueva.ToString(),
            HuboTransicion    = huboTransicion,
            Motivo            = motivo
        };
}
