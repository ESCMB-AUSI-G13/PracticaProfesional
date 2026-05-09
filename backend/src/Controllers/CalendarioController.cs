using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Calendario;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/calendario")]
[Authorize]
public class CalendarioController(
    ListarEventosCalendarioUseCase listar,
    CrearEventoCalendarioUseCase crear,
    ModificarEventoCalendarioUseCase modificar,
    EliminarEventoCalendarioUseCase eliminar) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventoCalendarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] int? anio, CancellationToken cancellationToken)
    {
        var resultado = await listar.EjecutarAsync(anio, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(EventoCalendarioDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear([FromBody] CrearEventoCalendarioDto dto, CancellationToken cancellationToken)
    {
        var resultado = await crear.EjecutarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { anio = resultado.FechaInicio.Year }, resultado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(typeof(EventoCalendarioDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Modificar(int id, [FromBody] ModificarEventoCalendarioDto dto, CancellationToken cancellationToken)
    {
        var resultado = await modificar.EjecutarAsync(id, dto, cancellationToken);
        return Ok(resultado);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Direccion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await eliminar.EjecutarAsync(id, cancellationToken);
        return NoContent();
    }
}
