using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Usuarios;

public class ReactivarUsuarioUseCase(IUsuarioRepository usuarioRepository)
{
    public async Task EjecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

        usuario.Reactivar();
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);
    }
}
