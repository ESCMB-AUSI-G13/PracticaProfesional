using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Reportes;
using PracticaProfesional.Application.Reportes.DTOs;

namespace PracticaProfesional.Controllers;

/// <summary>
/// Reportes de riesgo académico y retención por cohorte.
/// Acceso exclusivo para Dirección.
/// </summary>
[ApiController]
[Route("api/reportes")]
[Authorize(Roles = "Direccion")]
public class ReportesCohorteController(
    RiesgoAcademicoUseCase      riesgoUseCase,
    RetencionPorCohorteUseCase  retencionUseCase,
    TableroEjecutivoUseCase     tableroUseCase) : ControllerBase
{
    /// <summary>
    /// Tablero ejecutivo institucional — métricas globales para Dirección (RR-01).
    /// GET api/reportes/tablero-ejecutivo
    /// </summary>
    [HttpGet("tablero-ejecutivo")]
    [ProducesResponseType(typeof(TableroEjecutivoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> TableroEjecutivo(CancellationToken cancellationToken)
    {
        var resultado = await tableroUseCase.EjecutarAsync(cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Riesgo académico por estudiante (Bajo / Medio / Alto).
    /// GET api/reportes/riesgo-academico?anioCohorte=2024&amp;nivelRiesgo=Alto
    /// </summary>
    [HttpGet("riesgo-academico")]
    [ProducesResponseType(typeof(ReporteRiesgoAcademicoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RiesgoAcademico(
        [FromQuery] int?    anioCohorte,
        [FromQuery] int?    carreraId,
        [FromQuery] string? nivelRiesgo,
        CancellationToken   cancellationToken)
    {
        var filtro    = new FiltroRiesgoAcademicoDto(anioCohorte, carreraId, nivelRiesgo);
        var resultado = await riesgoUseCase.EjecutarAsync(filtro, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Tasas de retención, deserción y egreso agrupadas por cohorte (año de ingreso).
    /// GET api/reportes/retencion-cohorte?carreraId=1
    /// </summary>
    [HttpGet("retencion-cohorte")]
    [ProducesResponseType(typeof(ReporteRetencionCohorteDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RetencionCohorte(
        [FromQuery] int?  carreraId,
        CancellationToken cancellationToken)
    {
        var filtro    = new FiltroRetencionCohorteDto(carreraId);
        var resultado = await retencionUseCase.EjecutarAsync(filtro, cancellationToken);
        return Ok(resultado);
    }
}
