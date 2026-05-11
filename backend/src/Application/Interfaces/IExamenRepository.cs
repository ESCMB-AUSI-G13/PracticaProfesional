using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IExamenRepository
{
    Task<IEnumerable<Examen>> ListarAsync(CancellationToken cancellationToken = default);
    Task<Examen?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task AgregarAsync(Examen examen, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
    Task EliminarAsync(int id, CancellationToken cancellationToken = default);
}
