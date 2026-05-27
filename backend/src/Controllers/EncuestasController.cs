using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Encuestas;
using PracticaProfesional.Application.Encuestas.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/encuestas")]
[Authorize]
public class EncuestasController(
    ListarEncuestasUseCase           listarUseCase,
    CrearEncuestaUseCase             crearUseCase,
    AgregarPreguntaUseCase           agregarPreguntaUseCase,
    ActivarDesactivarEncuestaUseCase activarUseCase,
    ObtenerEncuestaPendienteUseCase  pendienteUseCase,
    ResponderEncuestaUseCase         responderUseCase,
    ResultadosEncuestasUseCase       resultadosUseCase,
    ListarEncuestasDocenteUseCase    listarDocenteUseCase,
    CrearEncuestaDocenteUseCase      crearDocenteUseCase,
    IEncuestaRepository              encuestaRepo,
    IEstudianteRepository            estudianteRepo) : ControllerBase
{
    // ── Dirección — gestión ───────────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = "Direccion")]
    public async Task<IActionResult> Listar(CancellationToken ct)
        => Ok(await listarUseCase.EjecutarAsync(ct));

    [HttpPost]
    [Authorize(Roles = "Direccion")]
    public async Task<IActionResult> Crear([FromBody] CrearEncuestaDto dto, CancellationToken ct)
        => Ok(await crearUseCase.EjecutarAsync(dto, ct));

    [HttpPost("preguntas")]
    [Authorize(Roles = "Direccion")]
    public async Task<IActionResult> AgregarPregunta(
        [FromBody] AgregarPreguntaDto dto, CancellationToken ct)
        => Ok(await agregarPreguntaUseCase.EjecutarAsync(dto, ct));

    [HttpPatch("{id}/activar")]
    [Authorize(Roles = "Direccion")]
    public async Task<IActionResult> Activar(int id, CancellationToken ct)
    {
        await activarUseCase.EjecutarAsync(id, activar: true, ct);
        return NoContent();
    }

    [HttpPatch("{id}/desactivar")]
    [Authorize(Roles = "Direccion")]
    public async Task<IActionResult> Desactivar(int id, CancellationToken ct)
    {
        await activarUseCase.EjecutarAsync(id, activar: false, ct);
        return NoContent();
    }

    // ── Docente — gestión de sus encuestas de evaluación ─────────────────────

    [HttpGet("docente/materias")]
    [Authorize(Roles = "Docente")]
    public async Task<IActionResult> MisMaterias(CancellationToken ct)
        => Ok(await listarDocenteUseCase.ObtenerMateriasAsync(ObtenerUsuarioId(), ct));

    [HttpGet("docente")]
    [Authorize(Roles = "Docente")]
    public async Task<IActionResult> MisEncuestas(CancellationToken ct)
        => Ok(await listarDocenteUseCase.EjecutarAsync(ObtenerUsuarioId(), ct));

    [HttpPost("docente")]
    [Authorize(Roles = "Docente")]
    public async Task<IActionResult> CrearDocente([FromBody] CrearEncuestaDto dto, CancellationToken ct)
        => Ok(await crearDocenteUseCase.EjecutarAsync(ObtenerUsuarioId(), dto, ct));

    [HttpPost("docente/preguntas")]
    [Authorize(Roles = "Docente")]
    public async Task<IActionResult> AgregarPreguntaDocente(
        [FromBody] AgregarPreguntaDto dto, CancellationToken ct)
    {
        var encuesta = await encuestaRepo.ObtenerConPreguntasAsync(dto.EncuestaId, ct);
        if (encuesta?.MateriaId is null) return NotFound();

        if (!await listarDocenteUseCase.EsMateriaDelDocenteAsync(ObtenerUsuarioId(), encuesta.MateriaId.Value, ct))
            return Forbid();

        return Ok(await agregarPreguntaUseCase.EjecutarAsync(dto, ct));
    }

    [HttpPatch("docente/{id}/activar")]
    [Authorize(Roles = "Docente")]
    public async Task<IActionResult> ActivarDocente(int id, CancellationToken ct)
    {
        var encuesta = await encuestaRepo.ObtenerConPreguntasAsync(id, ct);
        if (encuesta?.MateriaId is null) return NotFound();

        if (!await listarDocenteUseCase.EsMateriaDelDocenteAsync(ObtenerUsuarioId(), encuesta.MateriaId.Value, ct))
            return Forbid();

        await activarUseCase.EjecutarAsync(id, activar: true, ct);
        return NoContent();
    }

    [HttpPatch("docente/{id}/desactivar")]
    [Authorize(Roles = "Docente")]
    public async Task<IActionResult> DesactivarDocente(int id, CancellationToken ct)
    {
        var encuesta = await encuestaRepo.ObtenerConPreguntasAsync(id, ct);
        if (encuesta?.MateriaId is null) return NotFound();

        if (!await listarDocenteUseCase.EsMateriaDelDocenteAsync(ObtenerUsuarioId(), encuesta.MateriaId.Value, ct))
            return Forbid();

        await activarUseCase.EjecutarAsync(id, activar: false, ct);
        return NoContent();
    }

    // ── Estudiante — flujo de inscripción ────────────────────────────────────

    [HttpGet("pendiente")]
    [Authorize(Roles = "Estudiante")]
    public async Task<IActionResult> ObtenerPendiente(CancellationToken ct)
    {
        var usuarioId  = ObtenerUsuarioId();
        var estudiante = await estudianteRepo.ObtenerPorUsuarioIdAsync(usuarioId, ct);
        if (estudiante is null) return NoContent();

        var pendiente = await pendienteUseCase.EjecutarAsync(estudiante.Id, ct);
        return pendiente is null ? NoContent() : Ok(pendiente);
    }

    [HttpPost("responder")]
    [Authorize(Roles = "Estudiante")]
    public async Task<IActionResult> Responder(
        [FromBody] ResponderEncuestaDto dto, CancellationToken ct)
    {
        await responderUseCase.EjecutarAsync(ObtenerUsuarioId(), dto, ct);
        return NoContent();
    }

    // ── Docente — reportes de sus propias encuestas ───────────────────────────

    [HttpGet("docente/{id}/resultados")]
    [Authorize(Roles = "Docente")]
    public async Task<IActionResult> ResultadosDocente(int id, CancellationToken ct)
    {
        var encuesta = await encuestaRepo.ObtenerConPreguntasAsync(id, ct);
        if (encuesta?.MateriaId is null) return NotFound();

        if (!await listarDocenteUseCase.EsMateriaDelDocenteAsync(ObtenerUsuarioId(), encuesta.MateriaId.Value, ct))
            return Forbid();

        return Ok(await resultadosUseCase.ObtenerSatisfaccionAsync(id, ct));
    }

    [HttpGet("docente/comparativo")]
    [Authorize(Roles = "Docente")]
    public async Task<IActionResult> ComparativoDocente(CancellationToken ct)
    {
        var materias   = await listarDocenteUseCase.ObtenerMateriasAsync(ObtenerUsuarioId(), ct);
        var materiaIds = materias.Select(m => m.Id).ToList();
        return Ok(await resultadosUseCase.ObtenerComparativoDocenteAsync(materiaIds, ct));
    }

    // ── Dirección — reportes (RR-03, RR-04) ──────────────────────────────────

    [HttpGet("{id}/resultados")]
    [Authorize(Roles = "Direccion")]
    public async Task<IActionResult> Resultados(int id, CancellationToken ct)
        => Ok(await resultadosUseCase.ObtenerSatisfaccionAsync(id, ct));

    [HttpGet("comparativo")]
    [Authorize(Roles = "Direccion")]
    public async Task<IActionResult> Comparativo(CancellationToken ct)
        => Ok(await resultadosUseCase.ObtenerComparativoAsync(ct));

    // ─────────────────────────────────────────────────────────────────────────

    private int ObtenerUsuarioId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }
}
