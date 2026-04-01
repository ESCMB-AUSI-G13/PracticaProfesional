using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Preceptores;

public class DesactivarPreceptorUseCase(IUsuarioRepository usuarioRepository)
{
    public async Task EjecutarAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Preceptor no encontrado.");

        if (usuario.Rol != Rol.Preceptor)
            throw new InvalidOperationException("El usuario no es un preceptor.");

        usuario.Desactivar();
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);
    }
}
