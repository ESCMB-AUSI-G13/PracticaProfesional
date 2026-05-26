using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class EstudianteRepository(AppDbContext context) : IEstudianteRepository
{
    public async Task<Estudiante?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Estudiantes
            .Include(e => e.Usuario)
            .Include(e => e.Carrera)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<Estudiante?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default)
        => await context.Estudiantes
            .Include(e => e.Usuario)
            .Include(e => e.Carrera)
            .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId, cancellationToken);

    public async Task<IEnumerable<Estudiante>> ListarAsync(CancellationToken cancellationToken = default)
        => await context.Estudiantes
            .Include(e => e.Usuario)
            .Include(e => e.Carrera)
            .OrderBy(e => e.Usuario.Apellido)
            .ThenBy(e => e.Usuario.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<Estudiante?> ObtenerPorLegajoAsync(string legajo, CancellationToken cancellationToken = default)
        => await context.Estudiantes
            .Include(e => e.Usuario)
            .Include(e => e.Carrera)
            .FirstOrDefaultAsync(e => e.Usuario.Legajo == legajo, cancellationToken);

    public async Task AgregarAsync(Estudiante estudiante, CancellationToken cancellationToken = default)
    {
        await context.Estudiantes.AddAsync(estudiante, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task EliminarAsync(int estudianteId, int usuarioId, CancellationToken cancellationToken = default)
    {
        // Nullificar FKs opcionales en AuditoriaCambio antes de borrar inscripciones
        var inscMateriaIds = await context.InscripcionesMateria
            .Where(i => i.EstudianteId == estudianteId)
            .Select(i => i.Id)
            .ToListAsync(cancellationToken);

        var inscExamenIds = await context.InscripcionesExamen
            .Where(i => i.EstudianteId == estudianteId)
            .Select(i => i.Id)
            .ToListAsync(cancellationToken);

        if (inscMateriaIds.Count > 0)
            await context.AuditoriaCambios
                .Where(a => a.InscripcionMateriaId != null && inscMateriaIds.Contains(a.InscripcionMateriaId!.Value))
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.InscripcionMateriaId, (int?)null), cancellationToken);

        if (inscExamenIds.Count > 0)
            await context.AuditoriaCambios
                .Where(a => a.InscripcionExamenId != null && inscExamenIds.Contains(a.InscripcionExamenId!.Value))
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.InscripcionExamenId, (int?)null), cancellationToken);

        await context.Alertas
            .Where(a => a.EstudianteId == estudianteId)
            .ExecuteDeleteAsync(cancellationToken);

        await context.Asistencias
            .Where(a => a.EstudianteId == estudianteId)
            .ExecuteDeleteAsync(cancellationToken);

        await context.HistorialAcademico
            .Where(h => h.EstudianteId == estudianteId)
            .ExecuteDeleteAsync(cancellationToken);

        await context.InscripcionesExamen
            .Where(i => i.EstudianteId == estudianteId)
            .ExecuteDeleteAsync(cancellationToken);

        await context.InscripcionesMateria
            .Where(i => i.EstudianteId == estudianteId)
            .ExecuteDeleteAsync(cancellationToken);

        await context.Estudiantes
            .Where(e => e.Id == estudianteId)
            .ExecuteDeleteAsync(cancellationToken);

        await context.AuditoriaCambiosRol
            .Where(a => a.UsuarioId == usuarioId)
            .ExecuteDeleteAsync(cancellationToken);

        await context.AuditoriaCambios
            .Where(a => a.UsuarioId == usuarioId)
            .ExecuteDeleteAsync(cancellationToken);

        await context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId)
            .ExecuteDeleteAsync(cancellationToken);

        await context.Usuarios
            .Where(u => u.Id == usuarioId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
