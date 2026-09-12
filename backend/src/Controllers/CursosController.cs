using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Cursos;
using PracticaProfesional.Application.Cursos.DTOs;
using System.Security.Claims;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/cursos")]
[Authorize]
public class CursosController(
    CrearCursoUseCase crearCurso,
    ListarCursosUseCase listarCursos,
    ListarCursosPorMateriaUseCase listarCursosPorMateria,
    ListarCursosPreceptorUseCase listarCursosPreceptor,
    ModificarCursoUseCase modificarCurso,
    CerrarCursoUseCase cerrarCurso,
    ReactivarCursoUseCase reactivarCurso) : ControllerBase
{
    /// <summary>
    /// GET api/cursos — Dirección ve todos los cursos; un Estudiante solo ve los de su propia
    /// carrera (antes no filtraba nada, devolvía todos los cursos de todas las carreras).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Direccion,Estudiante")]
    [ProducesResponseType(typeof(IEnumerable<CursoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var esDireccion = User.IsInRole("Direccion");
        var resultado = await listarCursos.EjecutarAsync(usuarioId, esDireccion, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("por-materia/{materiaId:int}")]
    [Authorize(Roles = "Direccion,Estudiante")]
    [ProducesResponseType(typeof(IEnumerable<CursoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarPorMateria(int materiaId, CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var esDireccion = User.IsInRole("Direccion");
        var resultado = await listarCursosPorMateria.EjecutarAsync(materiaId, usuarioId, esDireccion, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>GET api/cursos/mis-cursos — cursos a cargo del preceptor autenticado (CU-33).</summary>
    [HttpGet("mis-cursos")]
    [Authorize(Roles = "Preceptor")]
    [ProducesResponseType(typeof(IEnumerable<CursoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MisCursos(CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var resultado = await listarCursosPreceptor.EjecutarAsync(usuarioId, cancellationToken);
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // "no encontrado" es BusinessException (400), no 404 — ver CHECKLIST.md Tier 7 #35
    public async Task<IActionResult> Modificar(int id, [FromBody] ModificarCursoDto dto, CancellationToken cancellationToken)
    {
        var resultado = await modificarCurso.EjecutarAsync(id, dto, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Ampliado a Preceptor: CLAUDE.md (CU-22/CU-33) exige explícitamente "control de períodos
    /// de inscripción y cierre de actas para Preceptores" — antes era Direccion-only (ver
    /// CHECKLIST.md, Tier 5 #20).
    /// </summary>
    [HttpPatch("{id:int}/cerrar")]
    [Authorize(Roles = "Direccion,Preceptor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // "no encontrado" es BusinessException (400), no 404 — ver CHECKLIST.md Tier 7 #35
    public async Task<IActionResult> Cerrar(int id, CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var esDireccion = User.IsInRole("Direccion");
        await cerrarCurso.EjecutarAsync(id, usuarioId, esDireccion, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/reactivar")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // "no encontrado" es BusinessException (400), no 404 — ver CHECKLIST.md Tier 7 #35
    public async Task<IActionResult> Reactivar(int id, CancellationToken cancellationToken)
    {
        await reactivarCurso.EjecutarAsync(id, cancellationToken);
        return NoContent();
    }
}
