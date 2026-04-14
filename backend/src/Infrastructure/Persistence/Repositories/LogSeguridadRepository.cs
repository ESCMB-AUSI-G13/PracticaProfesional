using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class LogSeguridadRepository(AppDbContext context) : ILogSeguridadRepository
{
    public async Task AgregarAsync(LogSeguridad log, CancellationToken cancellationToken = default)
    {
        await context.LogsSeguridad.AddAsync(log, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IEnumerable<LogSeguridad> Items, int Total)> ListarAsync(
        string? email,
        bool? soloFallidos,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int pagina,
        int tamanoPagina,
        CancellationToken cancellationToken = default)
    {
        var query = context.LogsSeguridad.AsQueryable();

        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(l => l.Email.Contains(email));

        if (soloFallidos == true)
            query = query.Where(l => !l.Exitoso);
        else if (soloFallidos == false)
            query = query.Where(l => l.Exitoso);

        if (fechaDesde.HasValue)
            query = query.Where(l => l.Timestamp >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(l => l.Timestamp <= fechaHasta.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
