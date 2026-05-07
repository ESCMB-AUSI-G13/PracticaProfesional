using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Materias;
using PracticaProfesional.Application.Materias.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/materias")]
[Authorize]
public class MateriasController(
    CrearMateriaUseCase crearMateria,
    ListarMateriasUseCase listarMaterias,
    ModificarMateriaUseCase modificarMateria) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Direccion,Estudiante")]
    [ProducesResponseType(typeof(IEnumerable<MateriaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var resultado = await listarMaterias.EjecutarAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(MateriaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearMateriaDto dto, CancellationToken cancellationToken)
    {
        var resultado = await crearMateria.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(MateriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Modificar(int id, [FromBody] ModificarMateriaDto dto, CancellationToken cancellationToken)
    {
        var resultado = await modificarMateria.EjecutarAsync(id, dto, cancellationToken);
        return Ok(resultado);
    }
}
