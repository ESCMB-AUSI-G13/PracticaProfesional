using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Auditoria;
using PracticaProfesional.Application.Auditoria.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/auditoria")]
[Authorize]
public class AuditoriaController(RegistrarCambioRolUseCase registrarCambioRol) : ControllerBase
{
    [HttpPost("cambio-rol")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarCambioRol(
        [FromBody] RegistrarCambioRolDto dto,
        CancellationToken cancellationToken)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        await registrarCambioRol.EjecutarAsync(usuarioId, dto, cancellationToken);
        return NoContent();
    }
}
