using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Infrastructure.Email;

public class EmailService(IConfiguration configuration) : IEmailService
{
    public async Task EnviarAlertaRiesgoAcademicoAsync(
        string destinatario,
        string nombreEstudiante,
        string tipoRiesgo,
        string detalle,
        CancellationToken cancellationToken = default)
    {
        var connectionString = configuration["AzureCommunication:ConnectionString"]!;
        var remitente = configuration["AzureCommunication:Remitente"]!;
        var emailClient = new EmailClient(connectionString);

        var mensaje = new EmailMessage(
            senderAddress: remitente,
            content: new EmailContent($"Alerta académica: {tipoRiesgo}")
            {
                Html = $"""
                    <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                      <h2 style="color: #c0392b;">&#9888; Alerta Académica</h2>
                      <p>Estimado/a <strong>{nombreEstudiante}</strong>,</p>
                      <p>Se ha detectado la siguiente situación de riesgo académico:</p>
                      <div style="background:#fdf2f2;border-left:4px solid #c0392b;padding:12px 16px;margin:20px 0;border-radius:4px;">
                        <strong>{tipoRiesgo}</strong><br/>{detalle}
                      </div>
                      <p>Le recomendamos tomar contacto con el área de preceptoría a la brevedad.</p>
                      <hr style="border:none;border-top:1px solid #ecf0f1;" />
                      <p style="color:#95a5a6;font-size:12px;">
                        Instituto Superior del Profesorado en Ciencias Económicas y Jurídicas
                      </p>
                    </div>
                    """
            },
            recipients: new EmailRecipients([new EmailAddress(destinatario)])
        );

        var operacion = await emailClient.SendAsync(WaitUntil.Completed, mensaje, cancellationToken);
        if (operacion.Value.Status == EmailSendStatus.Failed)
            throw new PracticaProfesional.Domain.Exceptions.BusinessException(
                $"Error al enviar alerta de riesgo (Azure): {operacion.Value.Status}", 500);
    }

    public async Task EnviarAlertaVencimientoAsync(
        string destinatario,
        string nombreDestinatario,
        string nombreEvento,
        DateTime fechaVencimiento,
        int diasRestantes,
        CancellationToken cancellationToken = default)
    {
        var connectionString = configuration["AzureCommunication:ConnectionString"]!;
        var remitente = configuration["AzureCommunication:Remitente"]!;
        var emailClient = new EmailClient(connectionString);

        var urgencia = diasRestantes == 0 ? "¡HOY VENCE!" : $"Vence en {diasRestantes} día{(diasRestantes == 1 ? "" : "s")}";
        var colorBorde = diasRestantes == 0 ? "#c0392b" : diasRestantes == 1 ? "#e67e22" : "#f39c12";

        var mensaje = new EmailMessage(
            senderAddress: remitente,
            content: new EmailContent($"Recordatorio: {nombreEvento} — {urgencia}")
            {
                Html = $"""
                    <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                      <h2 style="color: #2c3e50;">&#128197; Recordatorio de vencimiento</h2>
                      <p>Estimado/a <strong>{nombreDestinatario}</strong>,</p>
                      <p>Le recordamos que el siguiente plazo está próximo a vencer:</p>
                      <div style="background:#fef9e7;border-left:4px solid {colorBorde};padding:12px 16px;margin:20px 0;border-radius:4px;">
                        <strong>{nombreEvento}</strong><br/>
                        Fecha de vencimiento: <strong>{fechaVencimiento:dd/MM/yyyy}</strong><br/>
                        <span style="color:{colorBorde};font-weight:bold;">{urgencia}</span>
                      </div>
                      <p>Por favor, asegúrese de completar las acciones requeridas antes de la fecha indicada.</p>
                      <hr style="border:none;border-top:1px solid #ecf0f1;" />
                      <p style="color:#95a5a6;font-size:12px;">
                        Instituto Superior del Profesorado en Ciencias Económicas y Jurídicas
                      </p>
                    </div>
                    """
            },
            recipients: new EmailRecipients([new EmailAddress(destinatario)])
        );

        var operacion = await emailClient.SendAsync(WaitUntil.Completed, mensaje, cancellationToken);
        if (operacion.Value.Status == EmailSendStatus.Failed)
            throw new PracticaProfesional.Domain.Exceptions.BusinessException(
                $"Error al enviar alerta de vencimiento (Azure): {operacion.Value.Status}", 500);
    }

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
