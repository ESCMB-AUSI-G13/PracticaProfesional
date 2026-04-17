using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class AsistenciaRepository(AppDbContext context) : IAsistenciaRepository
{
    public async Task<(int Total, int AusentesInjustificados, int Presentes)> ObtenerEstadisticasAsync(
        int estudianteId,
        int materiaId,
        int cursoId,
        CancellationToken cancellationToken = default)
    {
        var registros = await context.Asistencias
            .Where(a =>
                a.EstudianteId == estudianteId &&
                a.MateriaId    == materiaId &&
                a.CursoId      == cursoId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total          = g.Count(),
                AusentesInjust = g.Count(a => a.Estado == EstadoAsistencia.Ausente),
                Presentes      = g.Count(a => a.Estado == EstadoAsistencia.Presente)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return registros is null
            ? (0, 0, 0)
            : (registros.Total, registros.AusentesInjust, registros.Presentes);
    }

    public async Task<DateTime?> ObtenerUltimaFechaActividadAsync(
        int estudianteId,
        CancellationToken cancellationToken = default)
        => await context.Asistencias
            .Where(a => a.EstudianteId == estudianteId)
            .MaxAsync(a => (DateTime?)a.Fecha, cancellationToken);

    public async Task<IEnumerable<Asistencia>> ObtenerConDetalleAsync(
        int? cursoId,
        int? materiaId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        bool soloAusencias,
        CancellationToken cancellationToken = default)
    {
        var query = context.Asistencias
            .Include(a => a.Estudiante).ThenInclude(e => e.Usuario)
            .Include(a => a.Materia)
            .Include(a => a.Curso)
            .AsQueryable();

        if (cursoId.HasValue)
            query = query.Where(a => a.CursoId == cursoId.Value);

        if (materiaId.HasValue)
            query = query.Where(a => a.MateriaId == materiaId.Value);

        if (fechaDesde.HasValue)
            query = query.Where(a => a.Fecha >= fechaDesde.Value.Date);

        if (fechaHasta.HasValue)
            query = query.Where(a => a.Fecha <= fechaHasta.Value.Date);

        if (soloAusencias)
            query = query.Where(a => a.Estado != EstadoAsistencia.Presente);

        return await query
            .OrderBy(a => a.Fecha)
            .ThenBy(a => a.Estudiante.Usuario.Apellido)
            .ThenBy(a => a.Estudiante.Usuario.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Asistencia>> ObtenerPorEstudianteAsync(
        int estudianteId,
        CancellationToken cancellationToken = default)
        => await context.Asistencias
            .Include(a => a.Materia)
            .Include(a => a.Curso)
            .Where(a => a.EstudianteId == estudianteId)
            .OrderBy(a => a.Materia.Nombre)
            .ThenBy(a => a.Fecha)
            .ToListAsync(cancellationToken);
}
