using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Usuarios;
using PracticaProfesional.Application.Usuarios.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "Direccion")]
public class UsuariosController(
    CrearUsuarioUseCase crearUsuario,
    ListarUsuariosUseCase listarUsuarios,
    ModificarUsuarioUseCase modificarUsuario,
    DesactivarUsuarioUseCase desactivarUsuario) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? rol, CancellationToken cancellationToken)
    {
        var resultado = await listarUsuarios.EjecutarAsync(rol, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioDto dto, CancellationToken cancellationToken)
    {
        var resultado = await crearUsuario.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Modificar(int id, [FromBody] ModificarUsuarioDto dto, CancellationToken cancellationToken)
    {
        var resultado = await modificarUsuario.EjecutarAsync(id, dto, cancellationToken);
        return Ok(resultado);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id, CancellationToken cancellationToken)
    {
        await desactivarUsuario.EjecutarAsync(id, cancellationToken);
        return NoContent();
    }
}
