using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class InscripcionExamenRepository(AppDbContext context) : IInscripcionExamenRepository
{
    public async Task<InscripcionExamen?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
        => await context.InscripcionesExamen
            .Include(i => i.Estudiante)
                .ThenInclude(e => e.Usuario)
            .Include(i => i.Examen)
                .ThenInclude(e => e.Materia)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<IEnumerable<InscripcionExamen>> ObtenerPorExamenAsync(
        int examenId,
        CancellationToken cancellationToken = default)
        => await context.InscripcionesExamen
            .AsNoTracking()
            .Include(i => i.Estudiante)
                .ThenInclude(e => e.Usuario)
            .Include(i => i.Examen)
                .ThenInclude(e => e.Materia)
            .Where(i => i.ExamenId == examenId)
            .OrderBy(i => i.Estudiante.Usuario.Apellido)
            .ThenBy(i => i.Estudiante.Usuario.Nombre)
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(InscripcionExamen inscripcion, CancellationToken cancellationToken = default)
    {
        await context.InscripcionesExamen.AddAsync(inscripcion, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AgregarRangoAsync(IEnumerable<InscripcionExamen> inscripciones, CancellationToken cancellationToken = default)
    {
        await context.InscripcionesExamen.AddRangeAsync(inscripciones, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExisteAsync(int estudianteId, int examenId, CancellationToken cancellationToken = default)
        => await context.InscripcionesExamen
            .AnyAsync(i => i.EstudianteId == estudianteId && i.ExamenId == examenId, cancellationToken);

    public async Task<IEnumerable<InscripcionExamen>> ListarPorEstudianteAsync(int estudianteId, CancellationToken cancellationToken = default)
        => await context.InscripcionesExamen
            .AsNoTracking()
            .Include(i => i.Examen).ThenInclude(e => e.Materia)
            .Where(i => i.EstudianteId == estudianteId)
            .OrderByDescending(i => i.Examen.FechaExamen)
            .ToListAsync(cancellationToken);

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
