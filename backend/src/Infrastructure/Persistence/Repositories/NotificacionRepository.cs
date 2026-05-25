using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class NotificacionRepository(AppDbContext context) : INotificacionRepository
{
    public async Task AgregarAsync(Notificacion notificacion, CancellationToken cancellationToken = default)
        => await context.Notificaciones.AddAsync(notificacion, cancellationToken);

    public async Task<Notificacion?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Notificaciones.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<IEnumerable<Notificacion>> ListarPorUsuarioAsync(
        int usuarioId, CancellationToken cancellationToken = default)
        => await context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.FechaCreacion)
            .Take(50)
            .ToListAsync(cancellationToken);

    public async Task MarcarTodasLeidasAsync(int usuarioId, CancellationToken cancellationToken = default)
        => await context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId && !n.Leida)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true), cancellationToken);

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
