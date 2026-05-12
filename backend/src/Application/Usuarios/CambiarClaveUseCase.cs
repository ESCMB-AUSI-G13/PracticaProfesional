using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Usuarios.DTOs;

namespace PracticaProfesional.Application.Usuarios;

public class CambiarClaveUseCase(
    IUsuarioRepository usuarioRepository,
    IAuditoriaService auditoria)
{
    public async Task EjecutarAsync(int id, CambiarClaveDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.NuevaClave) || dto.NuevaClave.Length < 6)
            throw new ArgumentException("La clave debe tener al menos 6 caracteres.");

        var usuario = await usuarioRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

        var hash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaClave);
        usuario.RestablecerPassword(hash);
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoria.RegistrarAsync("Usuario", id.ToString(), "CAMBIAR_CLAVE",
            valorAnterior: null,
            valorNuevo: new { Accion = "Clave actualizada por administrador" },
            cancellationToken);
    }
}
