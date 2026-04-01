using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class EstudianteRepository(AppDbContext context) : IEstudianteRepository
{
    public async Task<Estudiante?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default)
        => await context.Estudiantes
            .Include(e => e.Usuario)
            .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId, cancellationToken);

    public async Task<IEnumerable<Estudiante>> ListarAsync(CancellationToken cancellationToken = default)
        => await context.Estudiantes
            .Include(e => e.Usuario)
            .OrderBy(e => e.Usuario.Apellido)
            .ThenBy(e => e.Usuario.Nombre)
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(Estudiante estudiante, CancellationToken cancellationToken = default)
    {
        await context.Estudiantes.AddAsync(estudiante, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
