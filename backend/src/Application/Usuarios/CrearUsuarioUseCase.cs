using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Usuarios.DTOs;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Usuarios;

public class CrearUsuarioUseCase(
    IUsuarioRepository usuarioRepository,
    IAuditoriaService auditoria)
{
    public async Task<UsuarioDto> EjecutarAsync(CrearUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<Rol>(dto.Rol, ignoreCase: true, out var rol))
            throw new ArgumentException($"Rol inválido: {dto.Rol}");

        if (rol != Rol.Direccion)
            throw new BusinessException(
                $"El rol {rol} requiere un perfil adicional (Carrera, Categoría, Turno, etc.). " +
                "Creá este usuario desde la pantalla de Estudiantes, Docentes o Preceptores correspondiente.");

        if (await usuarioRepository.ExistePorDniAsync(dto.DNI, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese DNI.");

        if (await usuarioRepository.ExistePorEmailAsync(dto.Email, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        Usuario.ValidarFortalezaPassword(dto.Password);

        var legajo = await usuarioRepository.GenerarProximoLegajoAsync(cancellationToken);
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var usuario = Usuario.Crear(dto.DNI, legajo, dto.Email, dto.Nombre, dto.Apellido, passwordHash, rol);

        await usuarioRepository.AgregarAsync(usuario, cancellationToken);

        await auditoria.RegistrarAsync("Usuario", usuario.Id.ToString(), "CREAR",
            valorAnterior: null,
            valorNuevo: new { usuario.DNI, usuario.Legajo, usuario.Email, usuario.Nombre, usuario.Apellido, Rol = usuario.Rol.ToString() },
            cancellationToken);

        return ToDto(usuario);
    }

    internal static UsuarioDto ToDto(Usuario u) => new(
        u.Id, u.DNI, u.Legajo, u.Email, u.Nombre, u.Apellido, u.Rol.ToString(), u.Activo, u.FechaCreacion
    );
}
