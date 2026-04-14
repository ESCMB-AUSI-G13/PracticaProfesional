namespace PracticaProfesional.Application.Interfaces;

/// <summary>
/// Servicio de auditoría. Registra de forma inmutable cualquier cambio sobre datos críticos.
/// El ejecutor se resuelve automáticamente desde el contexto HTTP.
/// </summary>
public interface IAuditoriaService
{
    Task RegistrarAsync(
        string entidadTipo,
        string entidadId,
        string accion,
        object? valorAnterior = null,
        object? valorNuevo = null,
        CancellationToken cancellationToken = default);
}
