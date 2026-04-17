using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Calificaciones;
using PracticaProfesional.Application.Calificaciones.DTOs;

namespace PracticaProfesional.Controllers;

/// <summary>
/// Carga, rectificación y auditoría de calificaciones de exámenes (parciales y finales).
/// Acceso exclusivo para el rol Docente.
/// </summary>
[ApiController]
[Route("api/calificaciones")]
[Authorize(Roles = "Docente")]
public class CalificacionesController(
    CargarNotaExamenUseCase cargarNotaUseCase,
    ListarInscripcionesExamenUseCase listarInscripcionesUseCase,
    RectificarNotaExamenUseCase rectificarNotaUseCase,
    ObtenerHistorialNotasUseCase historialNotasUseCase) : ControllerBase
{
    /// <summary>
    /// GET api/calificaciones/examenes/{examenId}/inscripciones
    /// Devuelve el listado de alumnos inscriptos a un examen con su nota (si ya fue cargada).
    /// El docente usa este listado como acta antes de cargar las notas.
    /// </summary>
    [HttpGet("examenes/{examenId:int}/inscripciones")]
    [ProducesResponseType(typeof(IEnumerable<InscripcionExamenDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarInscripciones(
        int examenId,
        CancellationToken cancellationToken)
    {
        var resultado = await listarInscripcionesUseCase.EjecutarAsync(examenId, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// PUT api/calificaciones/examenes/inscripciones/{inscripcionExamenId}/nota
    /// Carga la nota obtenida por el estudiante en una inscripción a examen.
    ///
    /// Reglas:
    ///   - La inscripción debe estar en estado Activa.
    ///   - La nota debe estar entre 1 y 10.
    ///   - El cambio queda registrado de forma inmutable en Auditoría (CU-06).
    ///   - Una nota ya cargada (estado Aprobada/Desaprobada) no puede reemplazarse
    ///     por este endpoint; requiere el proceso de rectificación.
    /// </summary>
    [HttpPut("examenes/inscripciones/{inscripcionExamenId:int}/nota")]
    [ProducesResponseType(typeof(NotaExamenResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CargarNota(
        int inscripcionExamenId,
        [FromBody] CargarNotaRequestDto body,
        CancellationToken cancellationToken)
    {
        var dto = new CargarNotaExamenDto(inscripcionExamenId, body.Nota);
        var resultado = await cargarNotaUseCase.EjecutarAsync(dto, cancellationToken);
        return Ok(resultado);
    }
    /// <summary>
    /// PUT api/calificaciones/examenes/inscripciones/{inscripcionExamenId}/nota/rectificar
    /// Rectifica una nota ya cargada (estado Aprobada o Desaprobada).
    /// Requiere motivo obligatorio. El cambio queda registrado en Auditoría (CU-06)
    /// con ValorAnterior, ValorNuevo y el motivo de corrección.
    /// </summary>
    [HttpPut("examenes/inscripciones/{inscripcionExamenId:int}/nota/rectificar")]
    [ProducesResponseType(typeof(NotaExamenResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RectificarNota(
        int inscripcionExamenId,
        [FromBody] RectificarNotaRequestDto body,
        CancellationToken cancellationToken)
    {
        var dto = new RectificarNotaExamenDto(inscripcionExamenId, body.NuevaNota, body.Motivo);
        var resultado = await rectificarNotaUseCase.EjecutarAsync(dto, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// GET api/calificaciones/examenes/inscripciones/{inscripcionExamenId}/historial
    /// Devuelve el historial completo de cambios de nota para una inscripción:
    /// la carga inicial (CARGAR_NOTA) y todas las rectificaciones (RECTIFICAR_NOTA).
    /// </summary>
    [HttpGet("examenes/inscripciones/{inscripcionExamenId:int}/historial")]
    [ProducesResponseType(typeof(IEnumerable<CambioNotaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerHistorial(
        int inscripcionExamenId,
        CancellationToken cancellationToken)
    {
        var resultado = await historialNotasUseCase.EjecutarAsync(inscripcionExamenId, cancellationToken);
        return Ok(resultado);
    }
}

/// <summary>Body del request de carga de nota. Solo contiene el valor numérico.</summary>
public record CargarNotaRequestDto(decimal Nota);

/// <summary>Body del request de rectificación. Nueva nota + motivo obligatorio.</summary>
public record RectificarNotaRequestDto(decimal NuevaNota, string Motivo);
