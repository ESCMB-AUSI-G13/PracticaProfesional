using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Docentes;
using PracticaProfesional.Application.Docentes.DTOs;
using PracticaProfesional.Application.Usuarios;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/docentes")]
[Authorize(Roles = "Direccion")]
public class DocentesController(
    CrearDocenteUseCase crearDocente,
    ListarDocentesUseCase listarDocentes,
    ModificarDocenteUseCase modificarDocente,
    CambiarActivacionUseCase cambiarActivacion) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DocenteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var resultado = await listarDocentes.EjecutarAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DocenteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearDocenteDto dto, CancellationToken cancellationToken)
    {
        var resultado = await crearDocente.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{usuarioId:int}")]
    [ProducesResponseType(typeof(DocenteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Modificar(int usuarioId, [FromBody] ModificarDocenteDto dto, CancellationToken cancellationToken)
    {
        var resultado = await modificarDocente.EjecutarAsync(usuarioId, dto, cancellationToken);
        return Ok(resultado);
    }

    [HttpDelete("{usuarioId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int usuarioId, CancellationToken cancellationToken)
    {
        await cambiarActivacion.EjecutarAsync(usuarioId, activar: false, Rol.Docente, "Docente", cancellationToken);
        return NoContent();
    }

    [HttpPatch("{usuarioId:int}/reactivar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivar(int usuarioId, CancellationToken cancellationToken)
    {
        await cambiarActivacion.EjecutarAsync(usuarioId, activar: true, Rol.Docente, "Docente", cancellationToken);
        return NoContent();
    }
}
