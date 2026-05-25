using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Notificaciones;
using PracticaProfesional.Application.Notificaciones.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/notificaciones")]
[Authorize]
public class NotificacionesController(
    ObtenerMisNotificacionesUseCase obtenerUseCase,
    MarcarNotificacionLeidaUseCase marcarLeidaUseCase,
    MarcarTodasLeidasUseCase marcarTodasUseCase) : ControllerBase
{
    private int UsuarioId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificacionDto>>> MisNotificaciones(
        CancellationToken cancellationToken)
    {
        var resultado = await obtenerUseCase.EjecutarAsync(UsuarioId, cancellationToken);
        return Ok(resultado);
    }

    [HttpPatch("{id}/leida")]
    public async Task<IActionResult> MarcarLeida(int id, CancellationToken cancellationToken)
    {
        await marcarLeidaUseCase.EjecutarAsync(id, UsuarioId, cancellationToken);
        return NoContent();
    }

    [HttpPatch("marcar-todas-leidas")]
    public async Task<IActionResult> MarcarTodasLeidas(CancellationToken cancellationToken)
    {
        await marcarTodasUseCase.EjecutarAsync(UsuarioId, cancellationToken);
        return NoContent();
    }
}
