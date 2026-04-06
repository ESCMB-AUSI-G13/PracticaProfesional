using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Auth;
using PracticaProfesional.Application.Auth.DTOs;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    LoginUseCase loginUseCase,
    RegistroUseCase registroUseCase,
    SolicitarRestablecimientoUseCase solicitarRestablecimientoUseCase,
    RestablecerPasswordUseCase restablecerPasswordUseCase) : ControllerBase
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

    [HttpPost("olvide-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> OlvidePassword(
        [FromBody] SolicitarRestablecimientoRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await solicitarRestablecimientoUseCase.EjecutarAsync(request, cancellationToken);

        // En desarrollo se devuelve el enlace directo (sin necesitar SMTP)
        if (resultado.EnlaceDevMode is not null)
            return Ok(new { mensaje = "Modo desarrollo: usá el enlace a continuación.", enlace = resultado.EnlaceDevMode });

        return Ok(new { mensaje = "Si el correo está registrado, recibirás un enlace para restablecer tu contraseña." });
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] RestablecerPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await restablecerPasswordUseCase.EjecutarAsync(request, cancellationToken);
        return Ok(new { mensaje = "Contraseña restablecida correctamente." });
    }
}
