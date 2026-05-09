using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class CarreraRepository(AppDbContext context) : ICarreraRepository
{
    public async Task<IEnumerable<Carrera>> ListarAsync(CancellationToken cancellationToken = default)
        => await context.Carreras.OrderBy(c => c.Nombre).ToListAsync(cancellationToken);

    public async Task<Carrera?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Carreras.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<bool> ExistePorNombreAsync(string nombre, CancellationToken cancellationToken = default)
        => await context.Carreras.AnyAsync(c => c.Nombre == nombre.Trim(), cancellationToken);

    public async Task AgregarAsync(Carrera carrera, CancellationToken cancellationToken = default)
    {
        await context.Carreras.AddAsync(carrera, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
