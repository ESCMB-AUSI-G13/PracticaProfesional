using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Reportes;
using PracticaProfesional.Application.Reportes.DTOs;
using PracticaProfesional.Infrastructure.Pdf;

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
    TableroEjecutivoUseCase     tableroUseCase,
    RetencionAnualUseCase       retencionAnualUseCase,
    DesercionPorAnioUseCase     desercionPorAnioUseCase,
    EgresadosPorCarreraUseCase  egresadosUseCase,
    PdfReporteService           pdfService) : ControllerBase
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
    /// Años de cohorte que tienen al menos un estudiante (para deshabilitar opciones vacías).
    /// GET api/reportes/retencion-cohorte/anios?carreraId=1
    /// </summary>
    [HttpGet("retencion-cohorte/anios")]
    [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AniosCohorte(
        [FromQuery] int?  carreraId,
        CancellationToken cancellationToken)
    {
        var anios = await retencionUseCase.ObtenerAniosAsync(carreraId, cancellationToken);
        return Ok(anios);
    }

    /// <summary>
    /// Tasas de retención, deserción y egreso agrupadas por cohorte (año de ingreso).
    /// GET api/reportes/retencion-cohorte?carreraId=1
    /// </summary>
    [HttpGet("retencion-cohorte")]
    [ProducesResponseType(typeof(ReporteRetencionCohorteDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RetencionCohorte(
        [FromQuery] int?  carreraId,
        [FromQuery] int?  anioCohorte,
        CancellationToken cancellationToken)
    {
        var filtro    = new FiltroRetencionCohorteDto(carreraId, anioCohorte);
        var resultado = await retencionUseCase.EjecutarAsync(filtro, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Retención longitudinal por año de cursada (RR-12).
    /// Muestra qué % de cada cohorte continuó en Año 2, Año 3, Año 4, Año 5.
    /// GET api/reportes/retencion-anual?carreraId=1&amp;anioCohorte=2022
    /// </summary>
    [HttpGet("retencion-anual")]
    [ProducesResponseType(typeof(ReporteRetencionAnualDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RetencionAnual(
        [FromQuery] int?  carreraId,
        [FromQuery] int?  anioCohorte,
        CancellationToken cancellationToken)
    {
        var resultado = await retencionAnualUseCase.EjecutarAsync(carreraId, anioCohorte, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Tasa de deserción agrupada por año de cursada (1°, 2°, 3°, 4°).
    /// GET api/reportes/desercion-por-anio?carreraId=1&amp;anioCohorte=2022
    /// </summary>
    [HttpGet("desercion-por-anio")]
    [ProducesResponseType(typeof(ReporteDesercionPorAnioDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> DesercionPorAnio(
        [FromQuery] int?  carreraId,
        [FromQuery] int?  anioCohorte,
        CancellationToken cancellationToken)
    {
        var resultado = await desercionPorAnioUseCase.EjecutarAsync(carreraId, anioCohorte, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Egresados agrupados por carrera y año de cohorte.
    /// GET api/reportes/egresados-por-carrera?carreraId=1&amp;anioCohorte=2020
    /// </summary>
    [HttpGet("egresados-por-carrera")]
    [ProducesResponseType(typeof(ReporteEgresadosPorCarreraDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> EgresadosPorCarrera(
        [FromQuery] int?  carreraId,
        [FromQuery] int?  anioCohorte,
        CancellationToken cancellationToken)
    {
        var resultado = await egresadosUseCase.EjecutarAsync(carreraId, anioCohorte, cancellationToken);
        return Ok(resultado);
    }

    // ── Endpoints PDF ────────────────────────────────────────────────────────────

    /// <summary>GET api/reportes/tablero-ejecutivo/pdf</summary>
    [HttpGet("tablero-ejecutivo/pdf")]
    public async Task<IActionResult> TableroEjecutivoPdf(CancellationToken cancellationToken)
    {
        var data = await tableroUseCase.EjecutarAsync(cancellationToken);
        var pdf  = pdfService.GenerarTableroEjecutivo(data);
        return File(pdf, "application/pdf", "tablero-ejecutivo.pdf");
    }

    /// <summary>GET api/reportes/riesgo-academico/pdf</summary>
    [HttpGet("riesgo-academico/pdf")]
    public async Task<IActionResult> RiesgoAcademicoPdf(
        [FromQuery] int?    anioCohorte,
        [FromQuery] int?    carreraId,
        [FromQuery] string? nivelRiesgo,
        CancellationToken   cancellationToken)
    {
        var filtro = new FiltroRiesgoAcademicoDto(anioCohorte, carreraId, nivelRiesgo);
        var data   = await riesgoUseCase.EjecutarAsync(filtro, cancellationToken);
        var pdf    = pdfService.GenerarRiesgoAcademico(data);
        return File(pdf, "application/pdf", "riesgo-academico.pdf");
    }

    /// <summary>GET api/reportes/retencion-anual/pdf</summary>
    [HttpGet("retencion-anual/pdf")]
    public async Task<IActionResult> RetencionAnualPdf(
        [FromQuery] int?  carreraId,
        [FromQuery] int?  anioCohorte,
        CancellationToken cancellationToken)
    {
        var data = await retencionAnualUseCase.EjecutarAsync(carreraId, anioCohorte, cancellationToken);
        var pdf  = pdfService.GenerarRetencionAnual(data);
        return File(pdf, "application/pdf", "retencion-anual.pdf");
    }

    /// <summary>GET api/reportes/retencion-cohorte/pdf</summary>
    [HttpGet("retencion-cohorte/pdf")]
    public async Task<IActionResult> RetencionCohortePdf(
        [FromQuery] int?  carreraId,
        [FromQuery] int?  anioCohorte,
        CancellationToken cancellationToken)
    {
        var filtro = new FiltroRetencionCohorteDto(carreraId, anioCohorte);
        var data   = await retencionUseCase.EjecutarAsync(filtro, cancellationToken);
        var pdf    = pdfService.GenerarRetencionCohorte(data);
        return File(pdf, "application/pdf", "retencion-cohorte.pdf");
    }
}
