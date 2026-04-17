using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Reportes;
using PracticaProfesional.Application.Reportes.DTOs;

namespace PracticaProfesional.Controllers;

/// <summary>
/// Reportes operativos de inasistencias (RR-08, RR-09).
/// Accesible para Preceptores y Dirección.
/// </summary>
[ApiController]
[Route("api/reportes")]
[Authorize(Roles = "Preceptor,Direccion")]
public class ReportesOperativosController(
    ReporteInasistenciasUseCase reporteInasistencias,
    ControlIndividualPorLegajoUseCase controlPorLegajo) : ControllerBase
{
    /// <summary>
    /// RR-08: Reporte detallado de inasistencias.
    ///
    /// Devuelve el listado de ausencias (justificadas e injustificadas) con información
    /// del estudiante, materia, curso, fecha y motivo. Admite filtros opcionales.
    /// </summary>
    [HttpPost("inasistencias")]
    [ProducesResponseType(typeof(ReporteInasistenciasDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReporteInasistencias(
        [FromBody] FiltroInasistenciasDto filtro,
        CancellationToken cancellationToken)
    {
        var resultado = await reporteInasistencias.EjecutarAsync(filtro, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// RR-09: Control individual de asistencia por legajo.
    ///
    /// Devuelve el perfil del estudiante con el resumen de asistencia por cada materia
    /// cursada: totales, porcentajes y alertas de riesgo de pérdida de regularidad.
    /// </summary>
    [HttpGet("control-legajo/{legajo}")]
    [ProducesResponseType(typeof(ControlLegajoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ControlPorLegajo(
        string legajo,
        CancellationToken cancellationToken)
    {
        var resultado = await controlPorLegajo.EjecutarAsync(legajo, cancellationToken);
        return Ok(resultado);
    }
}
