namespace PracticaProfesional.Application.Interfaces;

public interface IEmailService
{
    Task EnviarResetPasswordAsync(string destinatario, string nombreCompleto, string token, CancellationToken cancellationToken = default);

    Task EnviarAlertaRiesgoAcademicoAsync(
        string destinatario,
        string nombreEstudiante,
        string tipoRiesgo,
        string detalle,
        CancellationToken cancellationToken = default);

    Task EnviarAlertaVencimientoAsync(
        string destinatario,
        string nombreDestinatario,
        string nombreEvento,
        DateTime fechaVencimiento,
        int diasRestantes,
        CancellationToken cancellationToken = default);
}
