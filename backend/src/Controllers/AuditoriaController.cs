using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Auditoria;
using PracticaProfesional.Application.Auditoria.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/auditoria")]
[Authorize]
public class AuditoriaController(
    RegistrarCambioRolUseCase registrarCambioRol,
    ListarAuditoriaLogsUseCase listarLogs) : ControllerBase
{
    /// <summary>
    /// Antes era Direccion-only, pero el frontend (AuthService.VISTAS_PERMITIDAS) también deja
    /// que un Docente active la vista "Estudiante" — esa llamada fallaba con 403 en silencio
    /// (sin manejo de error en el .subscribe()), así que ningún cambio de vista de Docente
    /// quedaba auditado. Ampliado a los mismos roles que hoy tienen vistas habilitadas en el
    /// frontend (ver CHECKLIST.md, Tier 5 #18).
    /// </summary>
    [HttpPost("cambio-rol")]
    [Authorize(Roles = "Direccion,Docente")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarCambioRol(
        [FromBody] RegistrarCambioRolDto dto,
        CancellationToken cancellationToken)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        await registrarCambioRol.EjecutarAsync(usuarioId, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Devuelve el historial de auditoría con filtros y paginación.
    /// Solo accesible por Direccion.
    /// </summary>
    [HttpGet("logs")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(PaginadoDto<AuditoriaLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerLogs(
        [FromQuery] string? entidadTipo,
        [FromQuery] string? accion,
        [FromQuery] string? ejecutorEmail,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 50,
        CancellationToken cancellationToken = default)
    {
        var filtro = new AuditoriaLogFiltroDto(
            entidadTipo, accion, ejecutorEmail,
            fechaDesde, fechaHasta,
            pagina, tamanoPagina);

        var resultado = await listarLogs.EjecutarAsync(filtro, cancellationToken);
        return Ok(resultado);
    }
}
