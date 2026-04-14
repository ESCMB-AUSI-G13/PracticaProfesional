using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Usuarios;

public class DesactivarUsuarioUseCase(
    IUsuarioRepository usuarioRepository,
    IAuditoriaService auditoria)
{
    public async Task EjecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

        usuario.Desactivar();
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoria.RegistrarAsync("Usuario", id.ToString(), "DESACTIVAR",
            valorAnterior: new { Activo = true, usuario.Email, usuario.Nombre, usuario.Apellido },
            valorNuevo:    new { Activo = false, usuario.Email, usuario.Nombre, usuario.Apellido },
            cancellationToken);
    }
}
