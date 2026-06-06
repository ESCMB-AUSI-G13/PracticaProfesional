using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Tests.Integration;

internal sealed class NoOpEmailService : IEmailService
{
    public Task EnviarResetPasswordAsync(string destinatario, string nombreCompleto, string token, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task EnviarAlertaRiesgoAcademicoAsync(string destinatario, string nombreEstudiante, string tipoRiesgo, string detalle, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task EnviarAlertaVencimientoAsync(string destinatario, string nombreDestinatario, string nombreEvento, DateTime fechaVencimiento, int diasRestantes, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
