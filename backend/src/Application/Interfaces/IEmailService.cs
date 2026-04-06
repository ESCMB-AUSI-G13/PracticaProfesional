namespace PracticaProfesional.Application.Interfaces;

public interface IEmailService
{
    Task EnviarResetPasswordAsync(string destinatario, string nombreCompleto, string token, CancellationToken cancellationToken = default);
}
