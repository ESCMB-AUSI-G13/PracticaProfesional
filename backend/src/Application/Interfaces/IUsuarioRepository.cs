using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistePorDniAsync(string dni, CancellationToken cancellationToken = default);
    Task<bool> ExistePorLegajoAsync(string legajo, CancellationToken cancellationToken = default);
    Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
