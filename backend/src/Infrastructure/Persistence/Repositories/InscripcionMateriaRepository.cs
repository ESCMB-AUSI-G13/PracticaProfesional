using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class InscripcionMateriaRepository(AppDbContext context) : IInscripcionMateriaRepository
{
    public async Task<bool> ExisteInscripcionActivaAsync(
        int estudianteId,
        int materiaId,
        CancellationToken cancellationToken = default)
        => await context.InscripcionesMateria
            .AnyAsync(i =>
                i.EstudianteId == estudianteId &&
                i.MateriaId == materiaId &&
                i.Estado == EstadoInscripcion.Activa,
            cancellationToken);

    public async Task<bool> TieneAlgunaInscripcionActivaAsync(
        int estudianteId,
        CancellationToken cancellationToken = default)
        => await context.InscripcionesMateria
            .AnyAsync(i =>
                i.EstudianteId == estudianteId &&
                i.Estado == EstadoInscripcion.Activa,
            cancellationToken);

    public async Task<InscripcionMateria?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.InscripcionesMateria
            .Include(i => i.Materia)
            .Include(i => i.Estudiante)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task AgregarAsync(InscripcionMateria inscripcion, CancellationToken cancellationToken = default)
    {
        await context.InscripcionesMateria.AddAsync(inscripcion, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
