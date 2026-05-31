using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IPadronRepository
{
    Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default);
    Task AgregarAsync(PadronAlumno padron, CancellationToken cancellationToken = default);
    Task<IEnumerable<PadronAlumno>> ListarAsync(CancellationToken cancellationToken = default);
    Task<bool> EliminarAsync(string dni, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
