using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Cursos.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class CursoRepository(AppDbContext context) : ICursoRepository
{
    public async Task<IEnumerable<CursoDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var raw = await context.Cursos
            .AsNoTracking()
            .OrderByDescending(c => c.Anio).ThenBy(c => c.Comision)
            .Select(c => new {
                c.Id, c.Anio, c.AnioLectivo, c.Comision, c.Cupo, c.Estado,
                c.PreceptorId,
                PreceptorNombre = c.Preceptor.Usuario.Nombre + " " + c.Preceptor.Usuario.Apellido
            })
            .ToListAsync(cancellationToken);

        return raw.Select(c => new CursoDto(
            c.Id, c.Anio, c.AnioLectivo, c.Comision, c.Cupo, c.Estado.ToString(),
            c.PreceptorId, c.PreceptorNombre));
    }

    public async Task<Curso?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Cursos
            .Include(c => c.Preceptor).ThenInclude(p => p.Usuario)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<bool> ExistePorAnioYComisionAsync(int anio, int anioLectivo, string comision, CancellationToken cancellationToken = default)
        => await context.Cursos.AnyAsync(c => c.Anio == anio && c.AnioLectivo == anioLectivo && c.Comision == comision.ToUpperInvariant(), cancellationToken);

    public async Task<bool> ExistePorAnioYComisionExcluyendoAsync(int anio, int anioLectivo, string comision, int excludeId, CancellationToken cancellationToken = default)
        => await context.Cursos.AnyAsync(c => c.Anio == anio && c.AnioLectivo == anioLectivo && c.Comision == comision.ToUpperInvariant() && c.Id != excludeId, cancellationToken);

    public async Task AgregarAsync(Curso curso, CancellationToken cancellationToken = default)
    {
        await context.Cursos.AddAsync(curso, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
