using Microsoft.Extensions.Configuration;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Auth;

public record SolicitarRestablecimientoRequest(string Email);

public record SolicitarRestablecimientoResult;

public class SolicitarRestablecimientoUseCase(
    IUsuarioRepository usuarioRepository,
    IEmailService emailService)
{
    public async Task<SolicitarRestablecimientoResult> EjecutarAsync(
        SolicitarRestablecimientoRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorEmailAsync(request.Email, cancellationToken);
        if (usuario is null || !usuario.Activo)
            return new SolicitarRestablecimientoResult();

        usuario.GenerarTokenReset();
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await emailService.EnviarResetPasswordAsync(
            destinatario: usuario.Email,
            nombreCompleto: $"{usuario.Nombre} {usuario.Apellido}",
            token: usuario.PasswordResetToken!,
            cancellationToken: cancellationToken);

        return new SolicitarRestablecimientoResult();
    }
}
