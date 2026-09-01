using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

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
        CancellationToken ct = default,
        int? usuarioIdSolicitante = null)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, ct)
            ?? throw new KeyNotFoundException($"{entidad} no encontrado.");

        // No tiene un caso de uso real: si querés dejar de ser Dirección, que te desactive otro
        // usuario de Dirección. Antes se permitía si quedaba otra cuenta Dirección activa, pero
        // eso corta tu propia sesión al instante con un error confuso en el frontend en vez de
        // un mensaje claro — ver validación manual de la Tarea 4 del Acta de Pruebas.
        if (!activar && usuarioIdSolicitante.HasValue && usuarioIdSolicitante.Value == usuarioId)
            throw new BusinessException(
                "No podés desactivar tu propia cuenta. Pedile a otro usuario de Dirección que lo haga.", 409);

        // Mensaje explícito con el rol esperado en vez de "El usuario no es un {entidad}" — con
        // entidad="Usuario" (endpoint /api/usuarios, rolEsperado=Direccion) ese texto daba
        // literalmente "El usuario no es un usuario.", que no dice nada accionable sobre qué
        // endpoint corresponde usar en su lugar (ver CHECKLIST.md, Tier 7 #39).
        if (rolEsperado.HasValue && usuario.Rol != rolEsperado.Value)
            throw new InvalidOperationException(
                $"El usuario no tiene el rol esperado ({rolEsperado}); su rol real es {usuario.Rol}.");

        // Evita que el sistema quede sin ningún usuario de Dirección activo (lockout total: sin
        // nadie que pueda reactivar cuentas ni administrar el sistema). No es solo un chequeo de
        // "no autodesactivarse" — aplica igual si Dirección A desactiva a Dirección B siendo la
        // última cuenta activa de ese rol.
        if (!activar && usuario.Rol == Rol.Direccion)
        {
            var direccionesActivas = (await usuarioRepository.ListarAsync(Rol.Direccion, ct))
                .Count(u => u.Activo && u.Id != usuarioId);
            if (direccionesActivas == 0)
                throw new BusinessException(
                    "No se puede desactivar: es el único usuario de Dirección activo. Debe haber al menos uno.", 409);
        }

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
