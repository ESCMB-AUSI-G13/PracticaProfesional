using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Asistencias;
using PracticaProfesional.Application.Asistencias.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/asistencias")]
[Authorize(Roles = "Docente")]
public class AsistenciasController(
    ObtenerEspaciosPorDocenteUseCase obtenerEspacios,
    ObtenerAlumnosPorEspacioUseCase obtenerAlumnos,
    RegistrarAsistenciasUseCase registrarAsistencias) : ControllerBase
{
    [HttpGet("mis-espacios")]
    [ProducesResponseType(typeof(IEnumerable<EspacioAsistenciaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MisEspacios(CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var resultado = await obtenerEspacios.EjecutarAsync(usuarioId, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("espacios/{espacioCurricularId:int}/alumnos")]
    [ProducesResponseType(typeof(IEnumerable<AlumnoParaAsistenciaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlumnosPorEspacio(int espacioCurricularId, CancellationToken cancellationToken)
    {
        var resultado = await obtenerAlumnos.EjecutarAsync(espacioCurricularId, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost]
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
}
