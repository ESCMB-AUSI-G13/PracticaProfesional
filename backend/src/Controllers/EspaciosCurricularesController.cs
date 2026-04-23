using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.EspaciosCurriculares;
using PracticaProfesional.Application.EspaciosCurriculares.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/espacios-curriculares")]
[Authorize(Roles = "Direccion")]
public class EspaciosCurricularesController(
    CrearEspacioCurricularUseCase crearUseCase,
    ListarEspaciosCurricularesUseCase listarUseCase,
    EliminarEspacioCurricularUseCase eliminarUseCase) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EspacioCurricularDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        => Ok(await listarUseCase.EjecutarAsync(cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(EspacioCurricularDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearEspacioCurricularDto dto, CancellationToken cancellationToken)
    {
        var resultado = await crearUseCase.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await eliminarUseCase.EjecutarAsync(id, cancellationToken);
        return NoContent();
    }
}
