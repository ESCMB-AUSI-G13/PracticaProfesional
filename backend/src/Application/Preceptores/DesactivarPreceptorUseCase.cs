using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Preceptores;

public class DesactivarPreceptorUseCase(
    IUsuarioRepository usuarioRepository,
    IAuditoriaService auditoria)
{
    public async Task EjecutarAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Preceptor no encontrado.");

        if (usuario.Rol != Rol.Preceptor)
            throw new InvalidOperationException("El usuario no es un preceptor.");

        usuario.Desactivar();
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoria.RegistrarAsync("Preceptor", usuarioId.ToString(), "DESACTIVAR",
            valorAnterior: new { Activo = true,  usuario.Email, usuario.Nombre, usuario.Apellido },
            valorNuevo:    new { Activo = false, usuario.Email, usuario.Nombre, usuario.Apellido },
            cancellationToken);
    }
}
