using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Auditoria.DTOs;
using PracticaProfesional.Application.LogsSeguridad;
using PracticaProfesional.Application.LogsSeguridad.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/logs-seguridad")]
[Authorize(Roles = "Direccion")]
public class LogsSeguridadController(ListarLogsLoginUseCase listarLogs) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginadoDto<LogSeguridadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? email,
        [FromQuery] bool? soloFallidos,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 50,
        CancellationToken cancellationToken = default)
    {
        var filtro = new LogSeguridadFiltroDto(email, soloFallidos, fechaDesde, fechaHasta, pagina, tamanoPagina);
        var resultado = await listarLogs.EjecutarAsync(filtro, cancellationToken);
        return Ok(resultado);
    }
}
