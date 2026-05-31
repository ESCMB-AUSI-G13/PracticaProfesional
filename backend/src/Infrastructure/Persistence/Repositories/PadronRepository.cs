using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class PadronRepository(AppDbContext context) : IPadronRepository
{
    public async Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default)
        => await context.PadronAlumnos.AnyAsync(p => p.DNI == dni, cancellationToken);

    public async Task AgregarAsync(PadronAlumno padron, CancellationToken cancellationToken = default)
    {
        await context.PadronAlumnos.AddAsync(padron, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<PadronAlumno>> ListarAsync(CancellationToken cancellationToken = default)
        => await context.PadronAlumnos
            .AsNoTracking()
            .OrderBy(p => p.DNI)
            .ToListAsync(cancellationToken);

    public async Task<bool> EliminarAsync(string dni, CancellationToken cancellationToken = default)
    {
        var padron = await context.PadronAlumnos.FindAsync([dni], cancellationToken);
        if (padron is null) return false;

        context.PadronAlumnos.Remove(padron);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
