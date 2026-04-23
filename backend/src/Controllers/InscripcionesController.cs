using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Inscripciones;
using PracticaProfesional.Application.Inscripciones.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/inscripciones")]
[Authorize(Roles = "Direccion")]
public class InscripcionesController(
    ListarInscripcionesUseCase listarUseCase,
    InscribirseEnMateriaUseCase inscribirseUseCase) : ControllerBase
{
    [HttpGet("materias")]
    [ProducesResponseType(typeof(IEnumerable<InscripcionMateriaListadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        => Ok(await listarUseCase.EjecutarAsync(cancellationToken));

    /// <summary>
    /// POST api/inscripciones/materias
    /// Inscribe a un estudiante en una materia validando correlatividades automáticamente (CU-22).
    /// </summary>
    [HttpPost("materias")]
    [ProducesResponseType(typeof(InscripcionMateriaResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InscribirseEnMateria(
        [FromBody] InscribirseEnMateriaDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await inscribirseUseCase.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
    }
}
