using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Auth;
using PracticaProfesional.Application.Auth.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(LoginUseCase loginUseCase, RegistroUseCase registroUseCase) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var resultado = await loginUseCase.EjecutarAsync(request, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("registro")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registro(
        [FromBody] RegistroRequestDto request,
        CancellationToken cancellationToken)
    {
        await registroUseCase.EjecutarAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created);
    }
}
