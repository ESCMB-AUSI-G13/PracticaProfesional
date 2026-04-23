using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Examenes;
using PracticaProfesional.Application.Examenes.DTOs;
using PracticaProfesional.Application.Inscripciones;
using PracticaProfesional.Application.Inscripciones.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/examenes")]
[Authorize]
public class ExamenesController(
    CrearExamenUseCase crearExamen,
    ListarExamenesUseCase listarExamenes,
    ListarFinalesDisponiblesUseCase listarFinales,
    InscribirseEnExamenUseCase inscribirseEnExamen) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Direccion,Docente")]
    [ProducesResponseType(typeof(IEnumerable<ExamenDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        => Ok(await listarExamenes.EjecutarAsync(cancellationToken));

    /// <summary>GET api/examenes/mis-finales — finales disponibles para el estudiante autenticado.</summary>
    [HttpGet("mis-finales")]
    [Authorize(Roles = "Estudiante")]
    [ProducesResponseType(typeof(IEnumerable<ExamenFinalDisponibleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MisFinales(CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        return Ok(await listarFinales.EjecutarAsync(usuarioId, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(ExamenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearExamenDto dto, CancellationToken cancellationToken)
    {
        var resultado = await crearExamen.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
    }

    /// <summary>POST api/examenes/{examenId}/inscripciones — estudiante se inscribe a un final (CU-33).</summary>
    [HttpPost("{examenId:int}/inscripciones")]
    [Authorize(Roles = "Estudiante,Direccion")]
    [ProducesResponseType(typeof(InscripcionExamenResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InscribirseEnFinal(
        int examenId,
        CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var dto = new InscribirseEnExamenDto(usuarioId, examenId);
        var resultado = await inscribirseEnExamen.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
    }
}

