using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Infrastructure.Persistence;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task EjecutarEnTransaccionAsync(
        Func<Task> operacion,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        // EnableRetryOnFailure exige envolver las transacciones manuales en una execution
        // strategy — de lo contrario EF Core lanza InvalidOperationException al abrir la
        // transacción (ver comentario histórico sobre este mismo problema en Program.cs).
        var estrategia = context.Database.CreateExecutionStrategy();

        // Llamadas totalmente calificadas: IExecutionStrategy.ExecuteAsync<TState,TResult> y
        // DatabaseFacade.BeginTransactionAsync(CancellationToken) son miembros de instancia con
        // el mismo nombre que las sobrecargas de extensión que necesitamos (Func<Task> y
        // BeginTransactionAsync(IsolationLevel, CancellationToken)) — su sola presencia bloquea
        // la resolución de esas extensiones aunque no matcheen los argumentos. Se invocan como
        // métodos estáticos para evitar la ambigüedad.
        Func<Task> intento = async () =>
        {
            await using var transaccion = await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions
                .BeginTransactionAsync(context.Database, isolationLevel, cancellationToken);
            try
            {
                await operacion();
                await transaccion.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaccion.RollbackAsync(cancellationToken);
                throw;
            }
        };
        await Microsoft.EntityFrameworkCore.ExecutionStrategyExtensions.ExecuteAsync(estrategia, intento);
    }
}
