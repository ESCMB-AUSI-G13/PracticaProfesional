using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class MateriaRepository(AppDbContext context) : IMateriaRepository
{
    public async Task<IEnumerable<Materia>> ListarAsync(CancellationToken cancellationToken = default)
        => await context.Materias.OrderBy(m => m.Plan).ThenBy(m => m.Nombre).ToListAsync(cancellationToken);

    public async Task<Materia?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Materias.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<bool> ExistePorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => await context.Materias.AnyAsync(m => m.Codigo == codigo.ToUpperInvariant(), cancellationToken);

    public async Task<bool> ExistePorCodigoExcluyendoAsync(string codigo, int excludeId, CancellationToken cancellationToken = default)
        => await context.Materias.AnyAsync(m => m.Codigo == codigo.ToUpperInvariant() && m.Id != excludeId, cancellationToken);

    public async Task AgregarAsync(Materia materia, CancellationToken cancellationToken = default)
    {
        await context.Materias.AddAsync(materia, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);

    public async Task<string?> ObtenerPlanAsync(int materiaId, CancellationToken cancellationToken = default)
        => await context.Materias.Where(m => m.Id == materiaId).Select(m => m.Plan).FirstOrDefaultAsync(cancellationToken);

    public async Task<int> ContarPorPlanAsync(string plan, CancellationToken cancellationToken = default)
        => await context.Materias.CountAsync(m => m.Plan == plan, cancellationToken);
}
