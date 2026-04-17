using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Inscripciones;
using PracticaProfesional.Application.Inscripciones.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/inscripciones")]
[Authorize]
public class InscripcionesController(InscribirseEnMateriaUseCase inscribirseUseCase) : ControllerBase
{
    /// <summary>
    /// POST api/inscripciones/materias
    /// Inscribe a un estudiante en una materia validando correlatividades automáticamente (CU-22).
    /// </summary>
    [HttpPost("materias")]
    public async Task<IActionResult> InscribirseEnMateria(
        [FromBody] InscribirseEnMateriaDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await inscribirseUseCase.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(InscribirseEnMateria), new { id = resultado.Id }, resultado);
    }
}
