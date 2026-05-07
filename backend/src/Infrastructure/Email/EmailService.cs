using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Infrastructure.Email;

public class EmailService(IConfiguration configuration) : IEmailService
{
    public async Task EnviarResetPasswordAsync(
        string destinatario,
        string nombreCompleto,
        string token,
        CancellationToken cancellationToken = default)
    {
        var frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:4200";
        var resetUrl = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}";
        var connectionString = configuration["AzureCommunication:ConnectionString"]!;
        var remitente = configuration["AzureCommunication:Remitente"]!;

        var emailClient = new EmailClient(connectionString);

        var mensaje = new EmailMessage(
            senderAddress: remitente,
            content: new EmailContent("Restablecimiento de contraseña")
            {
                Html = $"""
                    <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                      <h2 style="color: #2c3e50;">Restablecer contraseña</h2>
                      <p>Hola <strong>{nombreCompleto}</strong>,</p>
                      <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta.</p>
                      <p>Hacé clic en el botón para crear una nueva contraseña. El enlace es válido por <strong>1 hora</strong>.</p>
                      <div style="text-align: center; margin: 30px 0;">
                        <a href="{resetUrl}"
                           style="background-color: #4f46e5; color: white; padding: 12px 24px;
                                  text-decoration: none; border-radius: 6px; font-weight: bold;">
                          Restablecer contraseña
                        </a>
                      </div>
                      <p style="color: #7f8c8d; font-size: 13px;">
                        Si no solicitaste este cambio, ignorá este correo. Tu contraseña no será modificada.
                      </p>
                      <hr style="border: none; border-top: 1px solid #ecf0f1;" />
                      <p style="color: #95a5a6; font-size: 12px;">
                        Instituto Superior del Profesorado en Ciencias Económicas y Jurídicas
                      </p>
                    </div>
                    """
            },
            recipients: new EmailRecipients([new EmailAddress(destinatario)])
        );

        var operacion = await emailClient.SendAsync(WaitUntil.Completed, mensaje, cancellationToken);

        if (operacion.Value.Status == EmailSendStatus.Failed)
        {
            throw new PracticaProfesional.Domain.Exceptions.BusinessException(
                $"Error al enviar el correo (Azure Communication Services): {operacion.Value.Status}", 500);
        }
    }
}
