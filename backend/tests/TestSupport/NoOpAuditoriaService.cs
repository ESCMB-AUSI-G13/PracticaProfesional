using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Tests.TestSupport;

internal sealed class NoOpAuditoriaService : IAuditoriaService
{
    public Task RegistrarAsync(
        string entidadTipo,
        string entidadId,
        string accion,
        object? valorAnterior = null,
        object? valorNuevo = null,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
