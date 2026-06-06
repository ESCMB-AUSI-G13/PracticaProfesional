using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Usuarios;

public class CambiarActivacionUseCase(
    IUsuarioRepository usuarioRepository,
    IAuditoriaService  auditoria)
{
    public async Task EjecutarAsync(
        int  usuarioId,
        bool activar,
        Rol? rolEsperado,
        string entidad,
        CancellationToken ct = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, ct)
            ?? throw new KeyNotFoundException($"{entidad} no encontrado.");

        if (rolEsperado.HasValue && usuario.Rol != rolEsperado.Value)
            throw new InvalidOperationException($"El usuario no es un {entidad.ToLower()}.");

        if (activar) usuario.Reactivar();
        else         usuario.Desactivar();

        await usuarioRepository.GuardarCambiosAsync(ct);

        await auditoria.RegistrarAsync(
            entidad, usuarioId.ToString(), activar ? "REACTIVAR" : "DESACTIVAR",
            valorAnterior: new { Activo = !activar, usuario.Email, usuario.Nombre, usuario.Apellido },
            valorNuevo:    new { Activo = activar,  usuario.Email, usuario.Nombre, usuario.Apellido },
            ct);
    }
}
