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

    /// <summary>
    /// GET api/historial-academico/legajo/{legajo} — Preceptor/Dirección consultan el historial
    /// de un alumno puntual (mismo criterio de acceso que RR-09, control-legajo: búsqueda directa
    /// por legajo, sin restringir por curso a cargo). Antes de esto, el cierre de acta escribía
    /// la condición del alumno en HistorialAcademico pero nadie del staff podía verla.
    /// </summary>
    [HttpGet("legajo/{legajo}")]
    [Authorize(Roles = "Direccion,Preceptor")]
    [ProducesResponseType(typeof(MiHistorialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PorLegajo(string legajo, CancellationToken cancellationToken)
        => Ok(await obtenerMiHistorialUseCase.EjecutarPorLegajoAsync(legajo, cancellationToken));
}
