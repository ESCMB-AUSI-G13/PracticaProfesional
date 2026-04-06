using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Infrastructure.Email;

public class EmailService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IEmailService
{
    public async Task EnviarResetPasswordAsync(
        string destinatario,
        string nombreCompleto,
        string token,
        CancellationToken cancellationToken = default)
    {
        var frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:4200";
        var resetUrl = $"{frontendUrl}/reset-password?token={token}";
        var remitente = configuration["Resend:Remitente"] ?? "onboarding@resend.dev";
        var apiKey = configuration["Resend:ApiKey"]!;

        var payload = new
        {
            from = remitente,
            to = new[] { destinatario },
            subject = "Restablecimiento de contraseña",
            html = $"""
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
        };

        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.resend.com/emails", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var cuerpoError = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new PracticaProfesional.Domain.Exceptions.BusinessException(
                $"Error al enviar el correo (Resend {(int)response.StatusCode}): {cuerpoError}", 500);
        }
    }
}
