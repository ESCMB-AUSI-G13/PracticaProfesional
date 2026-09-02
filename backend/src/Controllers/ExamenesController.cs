using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Examenes;
using PracticaProfesional.Application.Examenes.DTOs;
using PracticaProfesional.Application.Inscripciones;
using PracticaProfesional.Application.Inscripciones.DTOs;
using System.Security.Claims;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/examenes")]
[Authorize]
public class ExamenesController(
    CrearExamenUseCase crearExamen,
    ListarExamenesUseCase listarExamenes,
    EliminarExamenUseCase eliminarExamen,
    ListarFinalesDisponiblesUseCase listarFinales,
    InscribirseEnExamenAutogestUseCase inscribirseEnExamenAutogest,
    ObtenerComprobanteInscripcionExamenUseCase comprobanteExamenUseCase) : ControllerBase
{
    /// <summary>GET api/examenes — Dirección ve todos los exámenes; Docente solo los de sus materias.</summary>
    [HttpGet]
    [Authorize(Roles = "Direccion,Docente")]
    [ProducesResponseType(typeof(IEnumerable<ExamenDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        int? docenteUsuarioId = User.IsInRole("Direccion")
            ? null
            : int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(await listarExamenes.EjecutarAsync(docenteUsuarioId, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await eliminarExamen.EjecutarAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>GET api/examenes/mis-finales — finales disponibles para el estudiante autenticado.</summary>
    [HttpGet("mis-finales")]
    [Authorize(Roles = "Estudiante")]
    [ProducesResponseType(typeof(IEnumerable<ExamenFinalDisponibleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MisFinales(CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        return Ok(await listarFinales.EjecutarAsync(usuarioId, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Direccion,Docente")]
    [ProducesResponseType(typeof(ExamenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearExamenDto dto, CancellationToken cancellationToken)
    {
        int? docenteUsuarioId = User.IsInRole("Docente")
            ? int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value)
            : null;
        var resultado = await crearExamen.EjecutarAsync(dto, docenteUsuarioId, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
    }

    /// <summary>
    /// POST api/examenes/{examenId}/inscripciones — el estudiante autenticado se autoinscribe a
    /// un final (CU-33). Para que Dirección inscriba a un estudiante puntual, usar
    /// POST api/inscripciones/examenes (recibe EstudianteId explícito — este endpoint no lo
    /// tenía y por eso era inutilizable para Dirección, ver CHECKLIST.md Tier 4 #12).
    /// </summary>
    [HttpPost("{examenId:int}/inscripciones")]
    [Authorize(Roles = "Estudiante")]
    [ProducesResponseType(typeof(InscripcionExamenResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InscribirseEnFinal(
        int examenId,
        CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var resultado = await inscribirseEnExamenAutogest.EjecutarAsync(usuarioId, examenId, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
    }

    /// <summary>GET api/examenes/inscripciones/{id}/comprobante — comprobante de confirmación de inscripción a examen.</summary>
    [HttpGet("inscripciones/{id:int}/comprobante")]
    [Authorize(Roles = "Estudiante,Direccion")]
    [ProducesResponseType(typeof(ComprobanteInscripcionExamenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerComprobanteInscripcion(int id, CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var comprobante = await comprobanteExamenUseCase.EjecutarAsync(id, usuarioId, User.IsInRole("Direccion"), cancellationToken);
        return Ok(comprobante);
    }
}

