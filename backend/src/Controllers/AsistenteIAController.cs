using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PracticaProfesional.Application.AsistenteIA;
using PracticaProfesional.Application.AsistenteIA.DTOs;

namespace PracticaProfesional.Controllers;

/// <summary>
/// Asistente de IA para Dirección — responde preguntas sobre datos institucionales
/// usando tool-calling sobre los reportes existentes. Acceso exclusivo para Dirección.
/// </summary>
[ApiController]
[Route("api/asistente-ia")]
[Authorize(Roles = "Direccion")]
[EnableRateLimiting("asistente-ia")]
public class AsistenteIAController(PreguntarAsistenteUseCase useCase) : ControllerBase
{
    /// <summary>
    /// POST api/asistente-ia/preguntar
    /// </summary>
    [HttpPost("preguntar")]
    [ProducesResponseType(typeof(AsistenteRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Preguntar(
        [FromBody] PreguntaAsistenteDto pregunta,
        CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var resultado = await useCase.EjecutarAsync(pregunta, usuarioId, cancellationToken);
        return Ok(resultado);
    }
}
