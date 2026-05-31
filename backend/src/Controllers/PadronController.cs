using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Padron;
using PracticaProfesional.Application.Padron.DTOs;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/padron")]
[Authorize(Roles = "Direccion")]
public class PadronController(
    CargarPadronUseCase cargarPadron,
    AgregarDniUseCase agregarDni,
    ListarPadronUseCase listarPadron,
    EliminarDniUseCase eliminarDni) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PadronAlumnoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var resultado = await listarPadron.EjecutarAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AgregarDni([FromBody] AgregarDniRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await agregarDni.EjecutarAsync(request.DNI, cancellationToken);
            return StatusCode(StatusCodes.Status201Created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
        catch (BusinessException ex)
        {
            return StatusCode(ex.StatusCode, new { detail = ex.Message });
        }
    }

    [HttpPost("importar")]
    [ProducesResponseType(typeof(ImportarPadronResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Importar(IFormFile archivo, CancellationToken cancellationToken)
    {
        if (archivo is null || archivo.Length == 0)
            return BadRequest(new { detail = "Debe adjuntar un archivo CSV." });

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (extension != ".csv" && extension != ".txt")
            return BadRequest(new { detail = "El archivo debe ser .csv o .txt." });

        List<string> dnis;
        using (var reader = new StreamReader(archivo.OpenReadStream()))
        {
            var contenido = await reader.ReadToEndAsync(cancellationToken);
            dnis = contenido
                .Split(['\n', '\r', ',', ';'], StringSplitOptions.RemoveEmptyEntries)
                .Select(d => d.Trim())
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .ToList();
        }

        if (dnis.Count == 0)
            return BadRequest(new { detail = "El archivo no contiene DNIs." });

        var resultado = await cargarPadron.EjecutarAsync(dnis, cancellationToken);
        return Ok(resultado);
    }

    [HttpDelete("{dni}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(string dni, CancellationToken cancellationToken)
    {
        try
        {
            await eliminarDni.EjecutarAsync(dni, cancellationToken);
            return NoContent();
        }
        catch (BusinessException ex)
        {
            return StatusCode(ex.StatusCode, new { detail = ex.Message });
        }
    }
}

public record AgregarDniRequest(string DNI);
