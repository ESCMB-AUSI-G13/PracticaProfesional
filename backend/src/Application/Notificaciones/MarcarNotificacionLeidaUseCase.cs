using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Notificaciones;

public class MarcarNotificacionLeidaUseCase(INotificacionRepository repo)
{
    public async Task EjecutarAsync(int id, int usuarioId, CancellationToken cancellationToken = default)
    {
        var notificacion = await repo.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException("Notificación no encontrada.", 404);

        if (notificacion.UsuarioId != usuarioId)
            throw new BusinessException("No tenés permiso para modificar esta notificación.", 403);

        notificacion.MarcarLeida();
        await repo.GuardarCambiosAsync(cancellationToken);
    }
}
