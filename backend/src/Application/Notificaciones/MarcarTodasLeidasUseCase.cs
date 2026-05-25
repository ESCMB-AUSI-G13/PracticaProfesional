using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Notificaciones;

public class MarcarTodasLeidasUseCase(INotificacionRepository repo)
{
    public async Task EjecutarAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        await repo.MarcarTodasLeidasAsync(usuarioId, cancellationToken);
    }
}
