using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class CorrelativiadadRepository(AppDbContext context) : ICorrelativiadadRepository
{
    public async Task<IEnumerable<Correlatividad>> ObtenerParaCursarAsync(
        int materiaDestinoId,
        CancellationToken cancellationToken = default)
        => await context.Correlatividades
            .Include(c => c.MateriaRequisito)
            .Where(c => c.MateriaDestinoId == materiaDestinoId && c.TipoRequerimiento == "Cursar")
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Correlatividad>> ObtenerParaRendirAsync(
        int materiaDestinoId,
        CancellationToken cancellationToken = default)
        => await context.Correlatividades
            .Include(c => c.MateriaRequisito)
            .Where(c => c.MateriaDestinoId == materiaDestinoId && c.TipoRequerimiento == "Rendir")
            .ToListAsync(cancellationToken);
}
