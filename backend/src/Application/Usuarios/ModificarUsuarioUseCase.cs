using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Usuarios.DTOs;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Usuarios;

public class ModificarUsuarioUseCase(IUsuarioRepository usuarioRepository)
{
    public async Task<UsuarioDto> EjecutarAsync(int id, ModificarUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

        if (!Enum.TryParse<Rol>(dto.Rol, ignoreCase: true, out var rol))
            throw new ArgumentException($"Rol inválido: {dto.Rol}");

        if (await usuarioRepository.ExistePorEmailExcluyendoIdAsync(dto.Email, id, cancellationToken))
            throw new InvalidOperationException("Ya existe otro usuario con ese email.");

        usuario.Modificar(dto.Nombre, dto.Apellido, dto.Email, rol);
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        return CrearUsuarioUseCase.ToDto(usuario);
    }
}
