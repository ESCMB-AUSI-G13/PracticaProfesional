using System.Data;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Tests.TestSupport;

/// <summary>
/// Ejecuta la operación directamente, sin abrir una transacción real. El proveedor InMemory
/// que usan estos tests no soporta transacciones de base de datos reales — la atomicidad en sí
/// (BeginTransactionAsync/Commit/Rollback) es una preocupación de infraestructura, no de la
/// lógica de negocio que estos tests verifican.
/// </summary>
internal sealed class NoOpUnitOfWork : IUnitOfWork
{
    public async Task EjecutarEnTransaccionAsync(
        Func<Task> operacion,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
        => await operacion();
}
