using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes;
using PracticaProfesional.Application.Reportes.DTOs;
using PracticaProfesional.Infrastructure.Pdf;
using System.Security.Claims;

namespace PracticaProfesional.Controllers;

/// <summary>
/// Reportes operativos de inasistencias (RR-08, RR-09).
/// RR-08: Preceptores, Dirección y Docentes (Docentes filtrados a sus espacios curriculares).
/// RR-09: Solo Preceptores y Dirección.
/// </summary>
[ApiController]
[Route("api/reportes")]
[Authorize(Roles = "Preceptor,Direccion,Docente")]
public class ReportesOperativosController(
    ReporteInasistenciasUseCase reporteInasistencias,
    ControlIndividualPorLegajoUseCase controlPorLegajo,
    IDocenteRepository docenteRepository,
    IPreceptorRepository preceptorRepository,
    IEspacioCurricularRepository espacioCurricularRepository,
    PdfReporteService pdfService) : ControllerBase
{
    /// <summary>
    /// RR-08: Reporte detallado de inasistencias.
    /// Si el llamante es Docente, se restringe automáticamente a sus espacios curriculares.
    /// </summary>
    [HttpPost("inasistencias")]
    [ProducesResponseType(typeof(ReporteInasistenciasDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReporteInasistencias(
        [FromBody] FiltroInasistenciasDto filtro,
        CancellationToken cancellationToken)
    {
        var espaciosPermitidos = await ObtenerEspaciosPermitidosAsync(cancellationToken);
        var resultado = await reporteInasistencias.EjecutarAsync(filtro, espaciosPermitidos, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Acota el reporte a lo que el llamante tiene a cargo. Dirección ve todo (null); el Docente,
    /// solo sus cátedras; el Preceptor, solo las cátedras dictadas en los cursos que tiene a cargo
    /// — antes el Preceptor no se acotaba y recibía los registros de todo el instituto.
    /// Devolver una lista vacía (y no null) es importante: significa "no tiene nada asignado, no
    /// ve nada", que es distinto de "sin restricción".
    /// </summary>
    private async Task<IReadOnlyList<(int MateriaId, int CursoId)>?> ObtenerEspaciosPermitidosAsync(
        CancellationToken cancellationToken)
    {
        if (User.IsInRole("Direccion")) return null;

        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        if (User.IsInRole("Docente"))
        {
            var docente = await docenteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
            if (docente is null) return [];

            var espacios = await espacioCurricularRepository.ListarPorDocenteIdAsync(docente.Id, cancellationToken);
            return espacios.Select(e => (e.MateriaId, e.CursoId)).ToList();
        }

        if (User.IsInRole("Preceptor"))
        {
            var preceptor = await preceptorRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
            if (preceptor is null) return [];

            var espacios = await espacioCurricularRepository.ListarPorPreceptorIdAsync(preceptor.Id, cancellationToken);
            return espacios.Select(e => (e.MateriaId, e.CursoId)).ToList();
        }

        return [];
    }

    /// <summary>
    /// RR-09: Control individual de asistencia por legajo.
    ///
    /// Devuelve el perfil del estudiante con el resumen de asistencia por cada materia
    /// cursada: totales, porcentajes y alertas de riesgo de pérdida de regularidad.
    /// </summary>
    [HttpGet("control-legajo/{legajo}")]
    [Authorize(Roles = "Preceptor,Direccion")]
    [ProducesResponseType(typeof(ControlLegajoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ControlPorLegajo(
        string legajo,
        CancellationToken cancellationToken)
    {
        var resultado = await controlPorLegajo.EjecutarAsync(legajo, cancellationToken);
        return Ok(resultado);
    }

    // ── Endpoints PDF ────────────────────────────────────────────────────────────

    /// <summary>GET api/reportes/control-legajo/{legajo}/pdf</summary>
    [HttpGet("control-legajo/{legajo}/pdf")]
    [Authorize(Roles = "Preceptor,Direccion")]
    public async Task<IActionResult> ControlPorLegajoPdf(
        string legajo,
        CancellationToken cancellationToken)
    {
        var data = await controlPorLegajo.EjecutarAsync(legajo, cancellationToken);
        var pdf  = pdfService.GenerarControlLegajo(data);
        return File(pdf, "application/pdf", $"control-legajo-{legajo}.pdf");
    }

    /// <summary>POST api/reportes/inasistencias/pdf</summary>
    [HttpPost("inasistencias/pdf")]
    public async Task<IActionResult> ReporteInasistenciasPdf(
        [FromBody] FiltroInasistenciasDto filtro,
        CancellationToken cancellationToken)
    {
        var espaciosPermitidos = await ObtenerEspaciosPermitidosAsync(cancellationToken);
        var data = await reporteInasistencias.EjecutarAsync(filtro, espaciosPermitidos, cancellationToken);
        var pdf  = pdfService.GenerarInasistencias(data);
        return File(pdf, "application/pdf", "reporte-inasistencias.pdf");
    }
}
