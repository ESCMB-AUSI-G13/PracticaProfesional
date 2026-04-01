using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IPreceptorRepository
{
    Task<Preceptor?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Preceptor>> ListarAsync(CancellationToken cancellationToken = default);
    Task AgregarAsync(Preceptor preceptor, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
