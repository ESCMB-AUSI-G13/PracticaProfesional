using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Usuarios;

public class ReactivarUsuarioUseCase(
    IUsuarioRepository usuarioRepository,
    IAuditoriaService auditoria)
{
    public async Task EjecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

        usuario.Reactivar();
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoria.RegistrarAsync("Usuario", id.ToString(), "REACTIVAR",
            valorAnterior: new { Activo = false, usuario.Email, usuario.Nombre, usuario.Apellido },
            valorNuevo:    new { Activo = true,  usuario.Email, usuario.Nombre, usuario.Apellido },
            cancellationToken);
    }
}
