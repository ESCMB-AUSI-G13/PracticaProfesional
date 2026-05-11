using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Exceptions;

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

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var examen = await db.Examenes.FindAsync([id], cancellationToken)
            ?? throw new BusinessException($"No se encontró el examen con Id {id}.");

        // 1. Desligar AuditoriaCambios que apuntan a las InscripcionesExamen de este examen
        var inscripcionIds = await db.InscripcionesExamen
            .Where(ie => ie.ExamenId == id)
            .Select(ie => ie.Id)
            .ToListAsync(cancellationToken);

        if (inscripcionIds.Count > 0)
        {
            await db.AuditoriaCambios
                .Where(a => a.InscripcionExamenId != null && inscripcionIds.Contains(a.InscripcionExamenId!.Value))
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.InscripcionExamenId, (int?)null), cancellationToken);

            // 2. Eliminar inscripciones al examen
            await db.InscripcionesExamen
                .Where(ie => ie.ExamenId == id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        // 3. Desligar AuditoriaCambios que apuntan directamente al examen
        await db.AuditoriaCambios
            .Where(a => a.ExamenId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.ExamenId, (int?)null), cancellationToken);

        // 4. Desligar Alertas que apuntan al examen
        await db.Alertas
            .Where(a => a.ExamenId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.ExamenId, (int?)null), cancellationToken);

        // 5. Eliminar el examen
        db.Examenes.Remove(examen);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistePorMateriaIdAsync(int materiaId, CancellationToken cancellationToken = default)
        => db.Examenes.AnyAsync(e => e.MateriaId == materiaId, cancellationToken);
}
