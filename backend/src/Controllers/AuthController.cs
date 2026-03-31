using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Auth;
using PracticaProfesional.Application.Auth.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(LoginUseCase loginUseCase) : ControllerBase
{
    /// <summary>
    /// Autentica un usuario y devuelve un token JWT.
    /// </summary>
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
}
