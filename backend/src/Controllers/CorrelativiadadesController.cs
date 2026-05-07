using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Correlatividades;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/correlatividades")]
[Authorize(Roles = "Direccion")]
public class CorrelativiadadesController(
    CrearCorrelativiadadUseCase crearCorrelatividad,
    ListarCorrelativiadadesUseCase listarCorrelatividades,
    EliminarCorrelativiadadUseCase eliminarCorrelatividad) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CorrelativiadadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] int materiaId,
        CancellationToken cancellationToken)
    {
        var resultado = await listarCorrelatividades.EjecutarAsync(materiaId, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CorrelativiadadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearCorrelativiadadDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await crearCorrelatividad.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { materiaId = resultado.MateriaDestinoId }, resultado);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await eliminarCorrelatividad.EjecutarAsync(id, cancellationToken);
        return NoContent();
    }
}
