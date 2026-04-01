using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class DocenteRepository(AppDbContext context) : IDocenteRepository
{
    public async Task<Docente?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default)
        => await context.Docentes
            .Include(d => d.Usuario)
            .FirstOrDefaultAsync(d => d.UsuarioId == usuarioId, cancellationToken);

    public async Task<IEnumerable<Docente>> ListarAsync(CancellationToken cancellationToken = default)
        => await context.Docentes
            .Include(d => d.Usuario)
            .OrderBy(d => d.Usuario.Apellido)
            .ThenBy(d => d.Usuario.Nombre)
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(Docente docente, CancellationToken cancellationToken = default)
    {
        await context.Docentes.AddAsync(docente, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
