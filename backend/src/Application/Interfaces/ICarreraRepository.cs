using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface ICarreraRepository
{
    Task<IEnumerable<Carrera>> ListarAsync(CancellationToken cancellationToken = default);
    Task<Carrera?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistePorNombreAsync(string nombre, CancellationToken cancellationToken = default);
    Task AgregarAsync(Carrera carrera, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
