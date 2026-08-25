using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/sesiones")]
[Authorize]
public class SesionesController(ISesionService sesionService) : ControllerBase
{
    private int UserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    /// <summary>
    /// El cliente autenticado envía este ping cada 30 segundos para mantenerse como "activo".
    /// </summary>
    [HttpPost("heartbeat")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Heartbeat()
    {
        if (UserId == 0) return Unauthorized();
        sesionService.RegistrarActividad(UserId);
        return NoContent();
    }

    /// <summary>
    /// El cliente llama esto al cerrar sesión para marcarse como inactivo de inmediato.
    /// </summary>
    [HttpDelete("heartbeat")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult CerrarSesion()
    {
        if (UserId == 0) return Unauthorized();
        sesionService.RemoverSesion(UserId);
        return NoContent();
    }

    /// <summary>
    /// Devuelve los IDs de usuarios con sesión activa en los últimos 60 segundos.
    /// Solo accesible por Dirección.
    /// </summary>
    [HttpGet("activas")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(IEnumerable<int>), StatusCodes.Status200OK)]
    public IActionResult ObtenerActivas()
        => Ok(sesionService.ObtenerIdsActivos());

    /// <summary>
    /// Cierra remotamente la sesión de un usuario: cualquier JWT que ya tenga emitido deja de
    /// aceptarse de inmediato (no hace falta desactivar la cuenta para esto). Antes Dirección
    /// no tenía ninguna forma de cortarle el acceso a una sesión puntual sin una desactivación
    /// completa (ver CHECKLIST.md, Tier 7 #38).
    /// </summary>
    [HttpDelete("activas/{usuarioId:int}")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult CerrarSesionRemota(int usuarioId)
    {
        sesionService.ForzarCierre(usuarioId);
        return NoContent();
    }
}
