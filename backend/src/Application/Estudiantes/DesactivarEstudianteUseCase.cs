using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Estudiantes;

public class DesactivarEstudianteUseCase(
    IUsuarioRepository usuarioRepository,
    IAuditoriaService auditoria)
{
    public async Task EjecutarAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Estudiante no encontrado.");

        if (usuario.Rol != Rol.Estudiante)
            throw new InvalidOperationException("El usuario no es un estudiante.");

        usuario.Desactivar();
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoria.RegistrarAsync("Estudiante", usuarioId.ToString(), "DESACTIVAR",
            valorAnterior: new { Activo = true,  usuario.Email, usuario.Nombre, usuario.Apellido },
            valorNuevo:    new { Activo = false, usuario.Email, usuario.Nombre, usuario.Apellido },
            cancellationToken);
    }
}
