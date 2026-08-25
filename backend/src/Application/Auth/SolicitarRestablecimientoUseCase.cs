using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Auth;

public record SolicitarRestablecimientoRequest(string Email);

public class SolicitarRestablecimientoUseCase(
    IUsuarioRepository usuarioRepository,
    IEmailService emailService,
    ILogger<SolicitarRestablecimientoUseCase> logger)
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

        // El endpoint siempre debe responder igual exista o no la cuenta (no filtra existencia
        // de email) y sin importar si el proveedor de correo está disponible — cualquier falla
        // del SDK externo (Azure Communication Services caído, credenciales mal configuradas,
        // etc.) se loguea acá y NO se propaga: antes escapaba sin capturar hasta el middleware
        // global, que exponía el mensaje interno de la excepción (ej. nombre del parámetro que
        // faltaba en el SDK) directo en la respuesta HTTP.
        try
        {
            await emailService.EnviarResetPasswordAsync(
                destinatario: usuario.Email,
                nombreCompleto: $"{usuario.Nombre} {usuario.Apellido}",
                token: usuario.PasswordResetToken!,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo enviar el email de restablecimiento de contraseña a {Email}.", usuario.Email);
        }
    }
}
