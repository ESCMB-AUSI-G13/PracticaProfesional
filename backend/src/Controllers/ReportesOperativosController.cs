using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes;
using PracticaProfesional.Application.Reportes.DTOs;
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
    IEspacioCurricularRepository espacioCurricularRepository) : ControllerBase
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
        IReadOnlyList<(int MateriaId, int CursoId)>? espaciosDocente = null;

        if (User.IsInRole("Docente"))
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var docente   = await docenteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
            if (docente is not null)
            {
                var espacios = await espacioCurricularRepository.ListarPorDocenteIdAsync(docente.Id, cancellationToken);
                espaciosDocente = espacios.Select(e => (e.MateriaId, e.CursoId)).ToList();
            }
        }

        var resultado = await reporteInasistencias.EjecutarAsync(filtro, espaciosDocente, cancellationToken);
        return Ok(resultado);
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
}
