using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.MiHistorial;
using PracticaProfesional.Application.MiHistorial.DTOs;
using System.Security.Claims;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/historial-academico")]
[Authorize]
public class HistorialAcademicoController(
    ObtenerMiHistorialUseCase obtenerMiHistorialUseCase) : ControllerBase
{
    /// <summary>GET api/historial-academico/mio — historial y condición actual del estudiante autenticado (CU-43).</summary>
    [HttpGet("mio")]
    [Authorize(Roles = "Estudiante")]
    [ProducesResponseType(typeof(MiHistorialDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Mio(CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(await obtenerMiHistorialUseCase.EjecutarAsync(usuarioId, cancellationToken));
    }
}
