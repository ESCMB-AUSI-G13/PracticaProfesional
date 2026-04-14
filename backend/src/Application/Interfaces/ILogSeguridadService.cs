namespace PracticaProfesional.Application.Interfaces;

/// <summary>
/// Registra intentos de login. Resuelve IP y UserAgent desde el contexto HTTP.
/// </summary>
public interface ILogSeguridadService
{
    Task RegistrarAsync(
        string email,
        bool exitoso,
        string? motivoFallo = null,
        CancellationToken cancellationToken = default);
}
