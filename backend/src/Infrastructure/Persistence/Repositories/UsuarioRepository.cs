using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class UsuarioRepository(AppDbContext context) : IUsuarioRepository
{
    public async Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        => await context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Usuarios.FindAsync([id], cancellationToken);

    public async Task<IEnumerable<Usuario>> ListarAsync(Rol? rol = null, CancellationToken cancellationToken = default)
    {
        var query = context.Usuarios.AsQueryable();
        if (rol.HasValue)
            query = query.Where(u => u.Rol == rol.Value);
        return await query.OrderBy(u => u.Apellido).ThenBy(u => u.Nombre).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistePorDniAsync(string dni, CancellationToken cancellationToken = default)
        => await context.Usuarios.AnyAsync(u => u.DNI == dni, cancellationToken);

    public async Task<bool> ExistePorLegajoAsync(string legajo, CancellationToken cancellationToken = default)
        => await context.Usuarios.AnyAsync(u => u.Legajo == legajo, cancellationToken);

    public async Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default)
        => await context.Usuarios.AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task<bool> ExistePorEmailExcluyendoIdAsync(string email, int idExcluir, CancellationToken cancellationToken = default)
        => await context.Usuarios.AnyAsync(u => u.Email == email.ToLowerInvariant() && u.Id != idExcluir, cancellationToken);

    public async Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await context.Usuarios.AddAsync(usuario, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
