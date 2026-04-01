using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Estudiantes;
using PracticaProfesional.Application.Estudiantes.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/estudiantes")]
[Authorize(Roles = "Direccion")]
public class EstudiantesController(
    CrearEstudianteUseCase crearEstudiante,
    ListarEstudiantesUseCase listarEstudiantes,
    ModificarEstudianteUseCase modificarEstudiante,
    DesactivarEstudianteUseCase desactivarEstudiante,
    ReactivarEstudianteUseCase reactivarEstudiante) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EstudianteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var resultado = await listarEstudiantes.EjecutarAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EstudianteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearEstudianteDto dto, CancellationToken cancellationToken)
    {
        var resultado = await crearEstudiante.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{usuarioId:int}")]
    [ProducesResponseType(typeof(EstudianteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Modificar(int usuarioId, [FromBody] ModificarEstudianteDto dto, CancellationToken cancellationToken)
    {
        var resultado = await modificarEstudiante.EjecutarAsync(usuarioId, dto, cancellationToken);
        return Ok(resultado);
    }

    [HttpDelete("{usuarioId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int usuarioId, CancellationToken cancellationToken)
    {
        await desactivarEstudiante.EjecutarAsync(usuarioId, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{usuarioId:int}/reactivar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivar(int usuarioId, CancellationToken cancellationToken)
    {
        await reactivarEstudiante.EjecutarAsync(usuarioId, cancellationToken);
        return NoContent();
    }
}
