using Microsoft.Extensions.Configuration;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Auth;

public record SolicitarRestablecimientoRequest(string Email);

public class SolicitarRestablecimientoUseCase(
    IUsuarioRepository usuarioRepository,
    IEmailService emailService)
{
    public async Task EjecutarAsync(
        SolicitarRestablecimientoRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorEmailAsync(request.Email, cancellationToken);
        if (usuario is null || !usuario.Activo)
            return;

        usuario.GenerarTokenReset();
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await emailService.EnviarResetPasswordAsync(
            destinatario: usuario.Email,
            nombreCompleto: $"{usuario.Nombre} {usuario.Apellido}",
            token: usuario.PasswordResetToken!,
            cancellationToken: cancellationToken);
    }
}
