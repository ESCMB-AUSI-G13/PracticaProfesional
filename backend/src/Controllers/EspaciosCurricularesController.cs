using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.EspaciosCurriculares;
using PracticaProfesional.Application.EspaciosCurriculares.DTOs;
using System.Security.Claims;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/espacios-curriculares")]
[Authorize(Roles = "Direccion,Docente")]
public class EspaciosCurricularesController(
    CrearEspacioCurricularUseCase crearUseCase,
    ListarEspaciosCurricularesUseCase listarUseCase,
    EliminarEspacioCurricularUseCase eliminarUseCase,
    ListarEspaciosDocenteUseCase listarEspaciosDocente) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(IEnumerable<EspacioCurricularDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        => Ok(await listarUseCase.EjecutarAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(EspacioCurricularDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearEspacioCurricularDto dto, CancellationToken cancellationToken)
    {
        var resultado = await crearUseCase.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
    }

    /// <summary>GET api/espacios-curriculares/mis-espacios — espacios del docente autenticado.</summary>
    [HttpGet("mis-espacios")]
    [Authorize(Roles = "Docente")]
    [ProducesResponseType(typeof(IEnumerable<EspacioCurricularDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MisEspacios(CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var resultado = await listarEspaciosDocente.EjecutarAsync(usuarioId, cancellationToken);
        return Ok(resultado);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await eliminarUseCase.EjecutarAsync(id, cancellationToken);
        return NoContent();
    }
}
