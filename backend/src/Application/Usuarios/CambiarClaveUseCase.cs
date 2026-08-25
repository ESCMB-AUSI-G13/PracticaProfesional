using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Usuarios.DTOs;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Usuarios;

public class CambiarClaveUseCase(
    IUsuarioRepository usuarioRepository,
    IAuditoriaService auditoria)
{
    public async Task EjecutarAsync(int id, CambiarClaveDto dto, CancellationToken cancellationToken = default)
    {
        Usuario.ValidarFortalezaPassword(dto.NuevaClave);

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
