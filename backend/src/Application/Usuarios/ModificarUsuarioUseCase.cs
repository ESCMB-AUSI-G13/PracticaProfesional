using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Usuarios.DTOs;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Usuarios;

public class ModificarUsuarioUseCase(
    IUsuarioRepository usuarioRepository,
    IAuditoriaService auditoria)
{
    public async Task<UsuarioDto> EjecutarAsync(int id, ModificarUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

        if (!Enum.TryParse<Rol>(dto.Rol, ignoreCase: true, out var rol))
            throw new ArgumentException($"Rol inválido: {dto.Rol}");

        if (await usuarioRepository.ExistePorEmailExcluyendoIdAsync(dto.Email, id, cancellationToken))
            throw new InvalidOperationException("Ya existe otro usuario con ese email.");

        var anterior = new { usuario.Email, usuario.Nombre, usuario.Apellido, Rol = usuario.Rol.ToString() };

        usuario.Modificar(dto.Nombre, dto.Apellido, dto.Email, rol);
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoria.RegistrarAsync("Usuario", id.ToString(), "MODIFICAR",
            valorAnterior: anterior,
            valorNuevo: new { usuario.Email, usuario.Nombre, usuario.Apellido, Rol = usuario.Rol.ToString() },
            cancellationToken);

        return CrearUsuarioUseCase.ToDto(usuario);
    }
}
