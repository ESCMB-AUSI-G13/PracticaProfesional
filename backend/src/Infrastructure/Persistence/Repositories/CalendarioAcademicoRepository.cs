using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class CalendarioAcademicoRepository(AppDbContext context) : ICalendarioAcademicoRepository
{
    public async Task<bool> EstaEnPeriodoAsync(
        TipoEvento tipo, DateTime fecha, CancellationToken cancellationToken = default)
        => await context.CalendarioAcademico
            .AnyAsync(e =>
                e.TipoEvento == tipo &&
                e.FechaInicio <= fecha.Date &&
                e.FechaFin >= fecha.Date,
            cancellationToken);

    public async Task<IEnumerable<CalendarioAcademico>> ListarAsync(
        int? anio = null, CancellationToken cancellationToken = default)
    {
        var query = context.CalendarioAcademico.AsQueryable();
        if (anio.HasValue)
            query = query.Where(e => e.FechaInicio.Year == anio || e.FechaFin.Year == anio);
        return await query.OrderBy(e => e.FechaInicio).ToListAsync(cancellationToken);
    }

    public async Task<CalendarioAcademico?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.CalendarioAcademico.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task AgregarAsync(CalendarioAcademico evento, CancellationToken cancellationToken = default)
    {
        await context.CalendarioAcademico.AddAsync(evento, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task EliminarAsync(CalendarioAcademico evento, CancellationToken cancellationToken = default)
    {
        context.CalendarioAcademico.Remove(evento);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);

    public async Task<bool> TieneEventosAsync(CancellationToken cancellationToken = default)
        => await context.CalendarioAcademico.AnyAsync(cancellationToken);
}
