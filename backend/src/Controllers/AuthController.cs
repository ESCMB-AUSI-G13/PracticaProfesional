using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Auth;
using PracticaProfesional.Application.Auth.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    LoginUseCase loginUseCase,
    RegistroUseCase registroUseCase,
    SolicitarRestablecimientoUseCase solicitarRestablecimientoUseCase,
    RestablecerPasswordUseCase restablecerPasswordUseCase,
    ILogSeguridadService logSeguridad) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await loginUseCase.EjecutarAsync(request, cancellationToken);

            await logSeguridad.RegistrarAsync(request.Email, exitoso: true,
                cancellationToken: cancellationToken);

            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            await logSeguridad.RegistrarAsync(request.Email, exitoso: false,
                motivoFallo: ex.Message, cancellationToken: cancellationToken);

            return Unauthorized(new { detail = ex.Message });
        }
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
        try
        {
            await solicitarRestablecimientoUseCase.EjecutarAsync(request, cancellationToken);
            return Ok(new { mensaje = "Si el correo está registrado, recibirás un enlace para restablecer tu contraseña." });
        }
        catch (BusinessException ex)
        {
            return StatusCode(ex.StatusCode, new { detail = ex.Message });
        }
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] RestablecerPasswordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await restablecerPasswordUseCase.EjecutarAsync(request, cancellationToken);
            return Ok(new { mensaje = "Contraseña restablecida correctamente." });
        }
        catch (BusinessException ex)
        {
            return StatusCode(ex.StatusCode, new { detail = ex.Message });
        }
    }
}
