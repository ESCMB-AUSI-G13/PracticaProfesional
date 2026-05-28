using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Auditoria.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class AuditoriaLogRepository(AppDbContext context) : IAuditoriaLogRepository
{
    public async Task AgregarAsync(AuditoriaLog log, CancellationToken cancellationToken = default)
    {
        await context.AuditoriaLogs.AddAsync(log, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IEnumerable<AuditoriaLog> Items, int Total)> ListarAsync(
        AuditoriaLogFiltroDto filtro,
        CancellationToken cancellationToken = default)
    {
        var query = context.AuditoriaLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.EntidadTipo))
            query = query.Where(l => l.EntidadTipo == filtro.EntidadTipo);

        if (!string.IsNullOrWhiteSpace(filtro.Accion))
            query = query.Where(l => l.Accion == filtro.Accion);

        if (!string.IsNullOrWhiteSpace(filtro.EjecutorEmail))
            query = query.Where(l => l.EjecutorEmail.Contains(filtro.EjecutorEmail));

        if (filtro.FechaDesde.HasValue)
            query = query.Where(l => l.Timestamp >= filtro.FechaDesde.Value);

        if (filtro.FechaHasta.HasValue)
            query = query.Where(l => l.Timestamp <= filtro.FechaHasta.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<IEnumerable<AuditoriaLog>> ObtenerPorEntidadAsync(
        string entidadTipo,
        string entidadId,
        CancellationToken cancellationToken = default)
        => await context.AuditoriaLogs
            .AsNoTracking()
            .Where(l => l.EntidadTipo == entidadTipo && l.EntidadId == entidadId)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync(cancellationToken);
}
