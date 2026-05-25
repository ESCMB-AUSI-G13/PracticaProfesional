using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface INotificacionRepository
{
    Task AgregarAsync(Notificacion notificacion, CancellationToken cancellationToken = default);
    Task<Notificacion?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notificacion>> ListarPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task MarcarTodasLeidasAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
