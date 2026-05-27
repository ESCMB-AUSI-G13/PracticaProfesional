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

    // ── Estudiante — flujo de inscripción ────────────────────────────────────

    /// <summary>
    /// Devuelve la encuesta pendiente del estudiante autenticado, o 204 si no hay.
    /// El frontend llama a este endpoint antes de confirmar cualquier inscripción.
    /// </summary>
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

    /// <summary>
    /// Registra la respuesta anónima. La identidad del estudiante se disocia
    /// mediante token SHA-256 — la respuesta no tiene FK al alumno.
    /// </summary>
    [HttpPost("responder")]
    [Authorize(Roles = "Estudiante")]
    public async Task<IActionResult> Responder(
        [FromBody] ResponderEncuestaDto dto, CancellationToken ct)
    {
        await responderUseCase.EjecutarAsync(ObtenerUsuarioId(), dto, ct);
        return NoContent();
    }

    // ── Dirección — reportes (RR-02, RR-03, RR-04) ───────────────────────────

    /// <summary>RR-03: Resultados de satisfacción de una encuesta.</summary>
    [HttpGet("{id}/resultados")]
    [Authorize(Roles = "Direccion")]
    public async Task<IActionResult> Resultados(int id, CancellationToken ct)
        => Ok(await resultadosUseCase.ObtenerSatisfaccionAsync(id, ct));

    /// <summary>RR-04: Comparativo entre todas las encuestas.</summary>
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
