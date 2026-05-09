using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Carreras;
using PracticaProfesional.Application.Carreras.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/carreras")]
[Authorize]
public class CarrerasController(ListarCarrerasUseCase listarCarreras) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CarreraDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var resultado = await listarCarreras.EjecutarAsync(cancellationToken);
        return Ok(resultado);
    }
}
