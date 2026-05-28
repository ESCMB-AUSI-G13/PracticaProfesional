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
            .AsNoTracking()
            .Include(c => c.MateriaRequisito)
            .Where(c => c.MateriaDestinoId == materiaDestinoId && c.TipoRequerimiento == "Cursar")
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Correlatividad>> ObtenerParaRendirAsync(
        int materiaDestinoId,
        CancellationToken cancellationToken = default)
        => await context.Correlatividades
            .AsNoTracking()
            .Include(c => c.MateriaRequisito)
            .Where(c => c.MateriaDestinoId == materiaDestinoId && c.TipoRequerimiento == "Rendir")
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Correlatividad>> ObtenerTodasPorTipoAsync(
        string tipoRequerimiento,
        CancellationToken cancellationToken = default)
        => await context.Correlatividades
            .AsNoTracking()
            .Where(c => c.TipoRequerimiento == tipoRequerimiento)
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(Correlatividad correlatividad, CancellationToken cancellationToken = default)
    {
        await context.Correlatividades.AddAsync(correlatividad, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Correlatividad>> ListarPorMateriaDestinoAsync(
        int materiaDestinoId,
        CancellationToken cancellationToken = default)
        => await context.Correlatividades
            .AsNoTracking()
            .Include(c => c.MateriaRequisito)
            .Where(c => c.MateriaDestinoId == materiaDestinoId)
            .OrderBy(c => c.TipoRequerimiento)
            .ToListAsync(cancellationToken);

    public async Task<Correlatividad?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Correlatividades
            .Include(c => c.MateriaRequisito)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task EliminarAsync(Correlatividad correlatividad, CancellationToken cancellationToken = default)
    {
        context.Correlatividades.Remove(correlatividad);
        await context.SaveChangesAsync(cancellationToken);
    }
}
