using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes;
using PracticaProfesional.Application.Reportes.DTOs;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Pdf;

namespace PracticaProfesional.Controllers;

/// <summary>
/// Reportes de rendimiento consolidado (RR-05, RR-06, RR-07).
/// Dirección: acceso completo. Docente: restringido a sus cátedras.
/// </summary>
[ApiController]
[Route("api/reportes/rendimiento")]
[Authorize(Roles = "Direccion,Docente")]
public class ReportesRendimientoController(
    ComparativoComisionesUseCase comparativoUseCase,
    EvolucionNotasUseCase        evolucionUseCase,
    PromediosCatedraUseCase      promediosUseCase,
    IDocenteRepository           docenteRepository,
    PdfReporteService            pdfService) : ControllerBase
{
    /// <summary>
    /// RR-05: Comparativo de rendimiento entre comisiones.
    /// GET api/reportes/rendimiento/comisiones?anio=2025&amp;materiaId=3
    /// </summary>
    [HttpGet("comisiones")]
    [ProducesResponseType(typeof(ReporteComparativoComisionesDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ComparativoComisiones(
        [FromQuery] int? materiaId,
        [FromQuery] int? anio,
        CancellationToken cancellationToken)
    {
        var docenteId = await ResolverDocenteIdSiAplicaAsync(cancellationToken);

        var filtro = new FiltroComparativoComisionesDto(materiaId, anio, docenteId);
        var resultado = await comparativoUseCase.EjecutarAsync(filtro, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// RR-06: Evolución de notas en el tiempo, agrupada por período (año-mes).
    /// GET api/reportes/rendimiento/evolucion?materiaId=3&amp;anio=2025
    /// </summary>
    [HttpGet("evolucion")]
    [ProducesResponseType(typeof(ReporteEvolucionNotasDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> EvolucionNotas(
        [FromQuery] int?        materiaId,
        [FromQuery] int?        anio,
        [FromQuery] int?        cuatrimestre,
        [FromQuery] byte?       anioCarrera,
        [FromQuery] TipoExamen? tipoExamen,
        [FromQuery] string      granularidad = "mensual",
        CancellationToken cancellationToken = default)
    {
        var gran = granularidad is "mensual" or "cuatrimestral" or "anual"
            ? granularidad : "mensual";

        var docenteId = await ResolverDocenteIdSiAplicaAsync(cancellationToken);

        var filtro = new FiltroEvolucionNotasDto(materiaId, anio, docenteId, cuatrimestre, anioCarrera, tipoExamen, gran);
        var resultado = await evolucionUseCase.EjecutarAsync(filtro, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// RR-07: Promedios por cátedra (Materia + Docente + Curso).
    /// GET api/reportes/rendimiento/catedras?anio=2025&amp;cursoId=2
    /// </summary>
    [HttpGet("catedras")]
    [ProducesResponseType(typeof(ReportePromediosCatedraDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> PromediosCatedra(
        [FromQuery] int? anio,
        [FromQuery] int? cursoId,
        CancellationToken cancellationToken)
    {
        var docenteId = await ResolverDocenteIdSiAplicaAsync(cancellationToken);

        var filtro = new FiltroPromediosCatedraDto(docenteId, anio, cursoId);
        var resultado = await promediosUseCase.EjecutarAsync(filtro, cancellationToken);
        return Ok(resultado);
    }

    // ── Endpoints PDF ────────────────────────────────────────────────────────────

    /// <summary>GET api/reportes/rendimiento/evolucion/pdf</summary>
    [HttpGet("evolucion/pdf")]
    public async Task<IActionResult> EvolucionNotasPdf(
        [FromQuery] int?        materiaId,
        [FromQuery] int?        anio,
        [FromQuery] int?        cuatrimestre,
        [FromQuery] byte?       anioCarrera,
        [FromQuery] TipoExamen? tipoExamen,
        [FromQuery] string      granularidad = "mensual",
        CancellationToken cancellationToken = default)
    {
        var gran = granularidad is "mensual" or "cuatrimestral" or "anual"
            ? granularidad : "mensual";

        var docenteId = await ResolverDocenteIdSiAplicaAsync(cancellationToken);
        var filtro    = new FiltroEvolucionNotasDto(materiaId, anio, docenteId, cuatrimestre, anioCarrera, tipoExamen, gran);
        var data      = await evolucionUseCase.EjecutarAsync(filtro, cancellationToken);
        var pdf       = pdfService.GenerarEvolucionNotas(data);
        return File(pdf, "application/pdf", "evolucion-notas.pdf");
    }

    /// <summary>GET api/reportes/rendimiento/catedras/pdf</summary>
    [HttpGet("catedras/pdf")]
    public async Task<IActionResult> PromediosCatedraPdf(
        [FromQuery] int? anio,
        [FromQuery] int? cursoId,
        CancellationToken cancellationToken)
    {
        var docenteId = await ResolverDocenteIdSiAplicaAsync(cancellationToken);
        var filtro    = new FiltroPromediosCatedraDto(docenteId, anio, cursoId);
        var data      = await promediosUseCase.EjecutarAsync(filtro, cancellationToken);
        var pdf       = pdfService.GenerarPromediosCatedra(data);
        return File(pdf, "application/pdf", "promedios-catedra.pdf");
    }

    /// <summary>GET api/reportes/rendimiento/comisiones/pdf</summary>
    [HttpGet("comisiones/pdf")]
    public async Task<IActionResult> ComparativoComisionesPdf(
        [FromQuery] int? materiaId,
        [FromQuery] int? anio,
        CancellationToken cancellationToken)
    {
        var docenteId = await ResolverDocenteIdSiAplicaAsync(cancellationToken);
        var filtro    = new FiltroComparativoComisionesDto(materiaId, anio, docenteId);
        var data      = await comparativoUseCase.EjecutarAsync(filtro, cancellationToken);
        var pdf       = pdfService.GenerarComparativoComisiones(data);
        return File(pdf, "application/pdf", "comparativo-comisiones.pdf");
    }

    // Si el usuario es Docente, resuelve su DocenteId para restringir la consulta.
    // Si es Dirección, retorna null (sin restricción).
    private async Task<int?> ResolverDocenteIdSiAplicaAsync(CancellationToken ct)
    {
        var rol = User.FindFirstValue(ClaimTypes.Role);
        if (rol != "Docente") return null;

        var usuarioIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdStr, out var usuarioId)) return null;

        var docente = await docenteRepository.ObtenerPorUsuarioIdAsync(usuarioId, ct);
        return docente?.Id;
    }
}
