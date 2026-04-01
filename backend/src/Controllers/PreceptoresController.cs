using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Preceptores;
using PracticaProfesional.Application.Preceptores.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/preceptores")]
[Authorize(Roles = "Direccion")]
public class PreceptoresController(
    CrearPreceptorUseCase crearPreceptor,
    ListarPreceptoresUseCase listarPreceptores,
    ModificarPreceptorUseCase modificarPreceptor,
    DesactivarPreceptorUseCase desactivarPreceptor,
    ReactivarPreceptorUseCase reactivarPreceptor) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PreceptorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var resultado = await listarPreceptores.EjecutarAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PreceptorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearPreceptorDto dto, CancellationToken cancellationToken)
    {
        var resultado = await crearPreceptor.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{usuarioId:int}")]
    [ProducesResponseType(typeof(PreceptorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Modificar(int usuarioId, [FromBody] ModificarPreceptorDto dto, CancellationToken cancellationToken)
    {
        var resultado = await modificarPreceptor.EjecutarAsync(usuarioId, dto, cancellationToken);
        return Ok(resultado);
    }

    [HttpDelete("{usuarioId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int usuarioId, CancellationToken cancellationToken)
    {
        await desactivarPreceptor.EjecutarAsync(usuarioId, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{usuarioId:int}/reactivar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivar(int usuarioId, CancellationToken cancellationToken)
    {
        await reactivarPreceptor.EjecutarAsync(usuarioId, cancellationToken);
        return NoContent();
    }
}
