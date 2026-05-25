using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Notificaciones.DTOs;

namespace PracticaProfesional.Application.Notificaciones;

public class ObtenerMisNotificacionesUseCase(INotificacionRepository repo)
{
    public async Task<IEnumerable<NotificacionDto>> EjecutarAsync(
        int usuarioId, CancellationToken cancellationToken = default)
    {
        var notificaciones = await repo.ListarPorUsuarioAsync(usuarioId, cancellationToken);

        return notificaciones.Select(n => new NotificacionDto
        {
            Id = n.Id,
            Titulo = n.Titulo,
            Mensaje = n.Mensaje,
            Leida = n.Leida,
            FechaCreacion = n.FechaCreacion,
            Tipo = n.Tipo?.ToString()
        });
    }
}
