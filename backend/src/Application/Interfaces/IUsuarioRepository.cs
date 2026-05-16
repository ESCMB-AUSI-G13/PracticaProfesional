using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Usuario>> ListarAsync(Rol? rol = null, CancellationToken cancellationToken = default);
    Task<string> GenerarProximoLegajoAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistePorDniAsync(string dni, CancellationToken cancellationToken = default);
    Task<bool> ExistePorLegajoAsync(string legajo, CancellationToken cancellationToken = default);
    Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistePorEmailExcluyendoIdAsync(string email, int idExcluir, CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerPorTokenResetAsync(string token, CancellationToken cancellationToken = default);
    Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
