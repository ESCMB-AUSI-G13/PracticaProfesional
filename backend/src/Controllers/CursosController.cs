using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Cursos;
using PracticaProfesional.Application.Cursos.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/cursos")]
[Authorize]
public class CursosController(
    CrearCursoUseCase crearCurso,
    ListarCursosUseCase listarCursos,
    ModificarCursoUseCase modificarCurso,
    CerrarCursoUseCase cerrarCurso,
    ReactivarCursoUseCase reactivarCurso) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Direccion,Estudiante")]
    [ProducesResponseType(typeof(IEnumerable<CursoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var resultado = await listarCursos.EjecutarAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(CursoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearCursoDto dto, CancellationToken cancellationToken)
    {
        var resultado = await crearCurso.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(CursoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Modificar(int id, [FromBody] ModificarCursoDto dto, CancellationToken cancellationToken)
    {
        var resultado = await modificarCurso.EjecutarAsync(id, dto, cancellationToken);
        return Ok(resultado);
    }

    [HttpPatch("{id:int}/cerrar")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cerrar(int id, CancellationToken cancellationToken)
    {
        await cerrarCurso.EjecutarAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/reactivar")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivar(int id, CancellationToken cancellationToken)
    {
        await reactivarCurso.EjecutarAsync(id, cancellationToken);
        return NoContent();
    }
}
