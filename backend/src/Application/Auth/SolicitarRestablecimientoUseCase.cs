using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Auth;

public record SolicitarRestablecimientoRequest(string Email);

public record SolicitarRestablecimientoResult(string? EnlaceDevMode);

public class SolicitarRestablecimientoUseCase(
    IUsuarioRepository usuarioRepository,
    IEmailService emailService,
    IHostEnvironment environment,
    IConfiguration configuration)
{
    public async Task<SolicitarRestablecimientoResult> EjecutarAsync(
        SolicitarRestablecimientoRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorEmailAsync(request.Email, cancellationToken);
        if (usuario is null || !usuario.Activo)
            return new SolicitarRestablecimientoResult(null);

        usuario.GenerarTokenReset();
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        var frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:4200";
        var enlace = $"{frontendUrl}/reset-password?token={usuario.PasswordResetToken}";

        if (environment.IsDevelopment())
        {
            // En desarrollo devolvemos el enlace directamente (no se necesita SMTP)
            return new SolicitarRestablecimientoResult(enlace);
        }

        await emailService.EnviarResetPasswordAsync(
            destinatario: usuario.Email,
            nombreCompleto: $"{usuario.Nombre} {usuario.Apellido}",
            token: usuario.PasswordResetToken!,
            cancellationToken: cancellationToken);

        return new SolicitarRestablecimientoResult(null);
    }
}
