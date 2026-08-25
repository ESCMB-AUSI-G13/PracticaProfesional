using System.Data;

namespace PracticaProfesional.Application.Interfaces;

/// <summary>
/// Envuelve una operación compuesta por varios pasos de repositorio (que hoy hacen su propio
/// SaveChanges) en una única transacción de base de datos: si un paso falla, los anteriores se
/// revierten. Usar <see cref="IsolationLevel.Serializable"/> cuando la operación incluye una
/// verificación de tipo "contar/leer y después insertar" (ej. validar cupo disponible) que de
/// otro modo queda expuesta a una condición de carrera bajo concurrencia real.
/// </summary>
public interface IUnitOfWork
{
    Task EjecutarEnTransaccionAsync(
        Func<Task> operacion,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}
