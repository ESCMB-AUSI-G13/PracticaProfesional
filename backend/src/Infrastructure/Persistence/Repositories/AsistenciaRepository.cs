using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes.DTOs;
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

    private IQueryable<Asistencia> ConstruirQueryInasistencias(
        int? cursoId,
        int? materiaId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        bool soloAusencias,
        string? comision,
        int? anioLectivo,
        IReadOnlyList<(int MateriaId, int CursoId)>? espaciosPermitidos)
    {
        var query = context.Asistencias
            .AsNoTracking()
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

        if (!string.IsNullOrWhiteSpace(comision))
            query = query.Where(a => a.Curso.Comision == comision.ToUpperInvariant());

        if (anioLectivo.HasValue)
            query = query.Where(a => a.Curso.AnioLectivo == anioLectivo.Value);

        // null = sin restricción (Dirección). Lista vacía = el llamante no tiene nada asignado, así
        // que no ve nada: antes la condición era `Count > 0`, con lo cual una lista vacía salteaba
        // el filtro entero y le devolvía los registros de todo el instituto.
        if (espaciosPermitidos is not null)
        {
            var materiaIds = espaciosPermitidos.Select(e => e.MateriaId).Distinct().ToList();
            var cursoIds   = espaciosPermitidos.Select(e => e.CursoId).Distinct().ToList();
            query = query.Where(a => materiaIds.Contains(a.MateriaId) && cursoIds.Contains(a.CursoId));
        }

        if (soloAusencias)
            query = query.Where(a => a.Estado != EstadoAsistencia.Presente);

        return query;
    }

    public async Task<(IReadOnlyList<Asistencia> Registros, int TotalRegistros, int TotalAusentes, int TotalAusentesJustificados, int TotalPresentes, IReadOnlyList<ConteoMateriaComisionDto> PorMateriaComision)> ObtenerConDetalleAsync(
        int? cursoId,
        int? materiaId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        bool soloAusencias,
        string? comision = null,
        int? anioLectivo = null,
        IReadOnlyList<(int MateriaId, int CursoId)>? espaciosPermitidos = null,
        int? pagina = null,
        int? tamanoPagina = null,
        CancellationToken cancellationToken = default)
    {
        var query = ConstruirQueryInasistencias(
            cursoId, materiaId, fechaDesde, fechaHasta, soloAusencias, comision, anioLectivo, espaciosPermitidos);

        // Totales y agrupación por materia/comisión sobre TODO lo que matchea el filtro,
        // calculados antes de recortar por página.
        var conteos = await query
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total          = g.Count(),
                Ausentes       = g.Count(a => a.Estado == EstadoAsistencia.Ausente),
                AusentesJust   = g.Count(a => a.Estado == EstadoAsistencia.AusenteJustificado),
                Presentes      = g.Count(a => a.Estado == EstadoAsistencia.Presente)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var porMateriaComision = await query
            .GroupBy(a => new { Materia = a.Materia.Nombre, Comision = a.Curso.Comision })
            .Select(g => new ConteoMateriaComisionDto
            {
                Materia  = g.Key.Materia,
                Comision = g.Key.Comision,
                Cantidad = g.Count()
            })
            .ToListAsync(cancellationToken);

        var ordenada = query
            .OrderBy(a => a.Fecha)
            .ThenBy(a => a.Estudiante.Usuario.Apellido)
            .ThenBy(a => a.Estudiante.Usuario.Nombre);

        var registros = pagina.HasValue && tamanoPagina.HasValue
            ? await ordenada.Skip((pagina.Value - 1) * tamanoPagina.Value).Take(tamanoPagina.Value).ToListAsync(cancellationToken)
            : await ordenada.ToListAsync(cancellationToken);

        return (
            registros,
            conteos?.Total ?? 0,
            conteos?.Ausentes ?? 0,
            conteos?.AusentesJust ?? 0,
            conteos?.Presentes ?? 0,
            porMateriaComision);
    }

    public async Task<IEnumerable<Asistencia>> ObtenerPorEstudianteAsync(
        int estudianteId,
        CancellationToken cancellationToken = default)
        => await context.Asistencias
            .AsNoTracking()
            .Include(a => a.Materia)
            .Include(a => a.Curso)
            .Where(a => a.EstudianteId == estudianteId)
            .OrderBy(a => a.Materia.Nombre)
            .ThenBy(a => a.Fecha)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistePorCursoMateriaFechaAsync(
        int cursoId,
        int materiaId,
        DateTime fecha,
        CancellationToken cancellationToken = default)
        => context.Asistencias.AnyAsync(
            a => a.CursoId == cursoId && a.MateriaId == materiaId && a.Fecha == fecha.Date,
            cancellationToken);

    public async Task RegistrarBulkAsync(
        IEnumerable<Asistencia> asistencias,
        CancellationToken cancellationToken = default)
    {
        await context.Asistencias.AddRangeAsync(asistencias, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Asistencia>> ObtenerPorEspacioYFechaAsync(
        int cursoId,
        int materiaId,
        DateTime fecha,
        CancellationToken cancellationToken = default)
        => await context.Asistencias
            .AsNoTracking()
            .Include(a => a.Estudiante).ThenInclude(e => e.Usuario)
            .Where(a => a.CursoId == cursoId && a.MateriaId == materiaId && a.Fecha == fecha.Date)
            .OrderBy(a => a.Estudiante.Usuario.Apellido)
            .ThenBy(a => a.Estudiante.Usuario.Nombre)
            .ToListAsync(cancellationToken);

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
