using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Asistencias;
using PracticaProfesional.Application.Asistencias.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/asistencias")]
[Authorize]
public class AsistenciasController(
    ObtenerEspaciosPorDocenteUseCase obtenerEspacios,
    ObtenerAlumnosPorEspacioUseCase obtenerAlumnos,
    RegistrarAsistenciasUseCase registrarAsistencias,
    ObtenerRegistroDelDiaUseCase obtenerRegistroDelDia,
    RectificarAsistenciaUseCase rectificarAsistencia) : ControllerBase
{
    [HttpGet("mis-espacios")]
    [Authorize(Roles = "Docente")]
    [ProducesResponseType(typeof(IEnumerable<EspacioAsistenciaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MisEspacios(CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var resultado = await obtenerEspacios.EjecutarAsync(usuarioId, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("espacios/{espacioCurricularId:int}/alumnos")]
    [Authorize(Roles = "Docente")]
    [ProducesResponseType(typeof(IEnumerable<AlumnoParaAsistenciaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlumnosPorEspacio(int espacioCurricularId, CancellationToken cancellationToken)
    {
        var resultado = await obtenerAlumnos.EjecutarAsync(espacioCurricularId, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost]
    [Authorize(Roles = "Docente")]
    [ProducesResponseType(typeof(ResumenAsistenciasDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarAsistenciasCommand command,
        CancellationToken cancellationToken)
    {
        var resultado = await registrarAsistencias.EjecutarAsync(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, resultado);
    }

    [HttpGet("espacios/{espacioCurricularId:int}/fecha/{fecha}")]
    [Authorize(Roles = "Docente,Preceptor")]
    [ProducesResponseType(typeof(RegistroDelDiaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistroDelDia(
        int espacioCurricularId,
        string fecha,
        CancellationToken cancellationToken)
    {
        if (!DateTime.TryParse(fecha, out var fechaParsed))
            return BadRequest("Formato de fecha inválido. Use yyyy-MM-dd.");

        var resultado = await obtenerRegistroDelDia.EjecutarAsync(espacioCurricularId, fechaParsed, cancellationToken);
        return Ok(resultado);
    }

    [HttpPut("rectificar")]
    [Authorize(Roles = "Docente,Preceptor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rectificar(
        [FromBody] RectificarAsistenciaCommand command,
        CancellationToken cancellationToken)
    {
        await rectificarAsistencia.EjecutarAsync(command, cancellationToken);
        return NoContent();
    }
}
