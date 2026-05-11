using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Inscripciones;
using PracticaProfesional.Application.Inscripciones.DTOs;
using System.Security.Claims;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/inscripciones")]
[Authorize]
public class InscripcionesController(
    ListarInscripcionesUseCase listarUseCase,
    InscribirseEnMateriaUseCase inscribirseUseCase,
    ObtenerComprobanteInscripcionUseCase comprobanteUseCase,
    ListarMisInscripcionesEstudianteUseCase misInscripcionesUseCase,
    InscribirseEnMateriaAutogestUseCase autogestUseCase,
    DarDeBajaInscripcionMateriaUseCase darDeBajaUseCase) : ControllerBase
{
    // ── Dirección ─────────────────────────────────────────────────────────────

    [HttpGet("materias")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(IEnumerable<InscripcionMateriaListadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        => Ok(await listarUseCase.EjecutarAsync(cancellationToken));

    /// <summary>
    /// POST api/inscripciones/materias
    /// Dirección inscribe a un estudiante en una materia (CU-22).
    /// </summary>
    [HttpPost("materias")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(InscripcionMateriaResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InscribirseEnMateria(
        [FromBody] InscribirseEnMateriaDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await inscribirseUseCase.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
    }

    [HttpGet("materias/{id}/comprobante")]
    [Authorize(Roles = "Direccion,Estudiante")]
    [ProducesResponseType(typeof(ComprobanteInscripcionMateriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerComprobante(int id, CancellationToken cancellationToken)
    {
        var comprobante = await comprobanteUseCase.EjecutarAsync(id, cancellationToken);
        return Ok(comprobante);
    }

    [HttpDelete("materias/{id:int}")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DarDeBajaInscripcion(int id, CancellationToken cancellationToken)
    {
        await darDeBajaUseCase.EjecutarAsync(id, cancellationToken);
        return NoContent();
    }

    // ── Estudiante (autogestionada) ───────────────────────────────────────────

    /// <summary>GET api/inscripciones/mis-materias — inscripciones activas del estudiante autenticado.</summary>
    [HttpGet("mis-materias")]
    [Authorize(Roles = "Estudiante")]
    [ProducesResponseType(typeof(IEnumerable<InscripcionMateriaListadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MisMaterias(CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(await misInscripcionesUseCase.EjecutarAsync(usuarioId, cancellationToken));
    }

    /// <summary>POST api/inscripciones/mis-materias — estudiante se auto-inscribe a una materia (CU-22).</summary>
    [HttpPost("mis-materias")]
    [Authorize(Roles = "Estudiante")]
    [ProducesResponseType(typeof(InscripcionMateriaResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InscribirseAutogest(
        [FromBody] InscribirseEnMateriaAutogestDto dto,
        CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var resultado = await autogestUseCase.EjecutarAsync(usuarioId, dto, cancellationToken);
        return CreatedAtAction(nameof(MisMaterias), new { id = resultado.Id }, resultado);
    }
}
