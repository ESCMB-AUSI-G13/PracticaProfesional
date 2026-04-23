using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class ExamenRepository(AppDbContext db) : IExamenRepository
{
    public async Task<IEnumerable<Examen>> ListarAsync(CancellationToken cancellationToken = default)
        => await db.Examenes
            .Include(e => e.Materia)
            .OrderByDescending(e => e.FechaExamen)
            .ToListAsync(cancellationToken);

    public async Task<Examen?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await db.Examenes
            .Include(e => e.Materia)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task AgregarAsync(Examen examen, CancellationToken cancellationToken = default)
    {
        await db.Examenes.AddAsync(examen, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await db.SaveChangesAsync(cancellationToken);
}
