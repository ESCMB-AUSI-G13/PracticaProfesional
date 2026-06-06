using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Estudiantes;
using PracticaProfesional.Application.Estudiantes.DTOs;
using PracticaProfesional.Application.Usuarios;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/estudiantes")]
[Authorize]
public class EstudiantesController(
    CrearEstudianteUseCase crearEstudiante,
    ListarEstudiantesUseCase listarEstudiantes,
    ModificarEstudianteUseCase modificarEstudiante,
    CambiarActivacionUseCase cambiarActivacion,
    EliminarEstudianteUseCase eliminarEstudiante) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(IEnumerable<EstudianteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var resultado = await listarEstudiantes.EjecutarAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("buscar")]
    [Authorize(Roles = "Direccion,Preceptor")]
    [ProducesResponseType(typeof(IEnumerable<EstudianteBusquedaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar(CancellationToken cancellationToken)
    {
        var estudiantes = await listarEstudiantes.EjecutarAsync(cancellationToken);
        var resultado = estudiantes.Select(e => new EstudianteBusquedaDto(e.Id, e.Nombre, e.Apellido, e.Legajo));
        return Ok(resultado);
    }

    [HttpPost]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(EstudianteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearEstudianteDto dto, CancellationToken cancellationToken)
    {
        var resultado = await crearEstudiante.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{usuarioId:int}")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(EstudianteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Modificar(int usuarioId, [FromBody] ModificarEstudianteDto dto, CancellationToken cancellationToken)
    {
        var resultado = await modificarEstudiante.EjecutarAsync(usuarioId, dto, cancellationToken);
        return Ok(resultado);
    }

    [HttpDelete("{usuarioId:int}")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int usuarioId, CancellationToken cancellationToken)
    {
        await cambiarActivacion.EjecutarAsync(usuarioId, activar: false, Rol.Estudiante, "Estudiante", cancellationToken);
        return NoContent();
    }

    [HttpPatch("{usuarioId:int}/reactivar")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivar(int usuarioId, CancellationToken cancellationToken)
    {
        await cambiarActivacion.EjecutarAsync(usuarioId, activar: true, Rol.Estudiante, "Estudiante", cancellationToken);
        return NoContent();
    }

    [HttpDelete("{usuarioId:int}/eliminar")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int usuarioId, CancellationToken cancellationToken)
    {
        await eliminarEstudiante.EjecutarAsync(usuarioId, cancellationToken);
        return NoContent();
    }
}
