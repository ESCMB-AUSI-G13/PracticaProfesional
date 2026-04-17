using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class MateriaRepository(AppDbContext context) : IMateriaRepository
{
    public async Task<string?> ObtenerPlanAsync(int materiaId, CancellationToken cancellationToken = default)
        => await context.Materias
            .Where(m => m.Id == materiaId)
            .Select(m => m.Plan)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<int> ContarPorPlanAsync(string plan, CancellationToken cancellationToken = default)
        => await context.Materias
            .CountAsync(m => m.Plan == plan, cancellationToken);
}
