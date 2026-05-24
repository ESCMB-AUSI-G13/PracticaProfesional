using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class AlertaRepository(AppDbContext context) : IAlertaRepository
{
    public async Task AgregarAsync(Alerta alerta, CancellationToken cancellationToken = default)
        => await context.Alertas.AddAsync(alerta, cancellationToken);

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);

    public async Task<bool> ExisteAlertaHoyAsync(
        int estudianteId, TipoAlerta tipo, CancellationToken cancellationToken = default)
    {
        var hoy = DateTime.UtcNow.Date;
        return await context.Alertas.AnyAsync(a =>
            a.EstudianteId == estudianteId &&
            a.Tipo == tipo &&
            a.FechaCreacion.Date == hoy,
            cancellationToken);
    }

    public async Task<bool> ExisteAlertaVencimientoHoyAsync(
        int calendarioId, string destinatario, CancellationToken cancellationToken = default)
    {
        var hoy = DateTime.UtcNow.Date;
        return await context.Alertas.AnyAsync(a =>
            a.CalendarioAcademicoId == calendarioId &&
            a.Destinatario == destinatario &&
            a.FechaCreacion.Date == hoy,
            cancellationToken);
    }

    public async Task<IEnumerable<Alerta>> ListarAsync(
        TipoAlerta? tipo = null,
        bool? enviada = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Alertas
            .Include(a => a.Estudiante).ThenInclude(e => e!.Usuario)
            .Include(a => a.CalendarioAcademico)
            .AsQueryable();

        if (tipo.HasValue)
            query = query.Where(a => a.Tipo == tipo);

        if (enviada.HasValue)
            query = query.Where(a => a.Enviada == enviada);

        return await query.OrderByDescending(a => a.FechaCreacion).ToListAsync(cancellationToken);
    }
}
