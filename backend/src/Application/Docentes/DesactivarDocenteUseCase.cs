using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Docentes;

public class DesactivarDocenteUseCase(
    IUsuarioRepository usuarioRepository,
    IAuditoriaService auditoria)
{
    public async Task EjecutarAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Docente no encontrado.");

        if (usuario.Rol != Rol.Docente)
            throw new InvalidOperationException("El usuario no es un docente.");

        usuario.Desactivar();
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoria.RegistrarAsync("Docente", usuarioId.ToString(), "DESACTIVAR",
            valorAnterior: new { Activo = true,  usuario.Email, usuario.Nombre, usuario.Apellido },
            valorNuevo:    new { Activo = false, usuario.Email, usuario.Nombre, usuario.Apellido },
            cancellationToken);
    }
}
