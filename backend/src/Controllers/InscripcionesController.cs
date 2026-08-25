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
    DarDeBajaInscripcionMateriaUseCase darDeBajaUseCase,
    InscribirseEnExamenUseCase inscribirseEnExamenUseCase) : ControllerBase
{
    // ── Dirección ─────────────────────────────────────────────────────────────

    [HttpGet("materias")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(IEnumerable<InscripcionMateriaListadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        => Ok(await listarUseCase.EjecutarAsync(cancellationToken));

    /// <summary>
    /// POST api/inscripciones/materias
    /// Dirección o Preceptor inscribe a un estudiante en una materia (CU-22). Ampliado a
    /// Preceptor para alinear con docs/casos-de-uso.md ("Inscripción a materia (manual) |
    /// Preceptor, Administrador") — ver CHECKLIST.md, Tier 5 #21.
    /// </summary>
    [HttpPost("materias")]
    [Authorize(Roles = "Direccion,Preceptor")]
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
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var comprobante = await comprobanteUseCase.EjecutarAsync(id, usuarioId, User.IsInRole("Direccion"), cancellationToken);
        return Ok(comprobante);
    }

    /// <summary>
    /// Ampliado a Preceptor y Estudiante para alinear con docs/casos-de-uso.md ("Dar de baja
    /// inscripción | Estudiante, Preceptor") — ver CHECKLIST.md, Tier 5 #21. Un Estudiante solo
    /// puede dar de baja su propia inscripción (chequeo de propiedad en el UseCase).
    /// </summary>
    [HttpDelete("materias/{id:int}")]
    [Authorize(Roles = "Direccion,Preceptor,Estudiante")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DarDeBajaInscripcion(int id, CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var esStaff = User.IsInRole("Direccion") || User.IsInRole("Preceptor");
        await darDeBajaUseCase.EjecutarAsync(id, usuarioId, esStaff, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// POST api/inscripciones/examenes — Dirección inscribe a un estudiante puntual a un examen
    /// (CU-33), con EstudianteId explícito. Antes el único endpoint de inscripción a examen
    /// resolvía siempre el estudiante desde el token de quien llama, así que Dirección no tenía
    /// ningún camino real para usarlo (ver CHECKLIST.md, Tier 4 #12).
    /// </summary>
    [HttpPost("examenes")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(InscripcionExamenResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InscribirseEnExamen(
        [FromBody] InscribirseEnExamenDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await inscribirseEnExamenUseCase.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
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
