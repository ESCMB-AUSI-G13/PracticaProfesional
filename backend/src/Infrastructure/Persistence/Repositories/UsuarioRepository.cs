using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class UsuarioRepository(AppDbContext context) : IUsuarioRepository
{
    public async Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        => await context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task<bool> ExistePorDniAsync(string dni, CancellationToken cancellationToken = default)
        => await context.Usuarios.AnyAsync(u => u.DNI == dni, cancellationToken);

    public async Task<bool> ExistePorLegajoAsync(string legajo, CancellationToken cancellationToken = default)
        => await context.Usuarios.AnyAsync(u => u.Legajo == legajo, cancellationToken);

    public async Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default)
        => await context.Usuarios.AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await context.Usuarios.AddAsync(usuario, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
