using PracticaProfesional.Application.Auth.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Auth;

public class RegistroUseCase(IUsuarioRepository usuarioRepository)
{
    private static readonly HashSet<Rol> RolesPermitidos = [Rol.Estudiante, Rol.Docente];

    public async Task EjecutarAsync(RegistroRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<Rol>(dto.Rol, ignoreCase: true, out var rol) || !RolesPermitidos.Contains(rol))
            throw new ArgumentException("El auto-registro solo está disponible para los roles Estudiante y Docente.");

        if (await usuarioRepository.ExistePorDniAsync(dto.DNI, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese DNI.");

        if (await usuarioRepository.ExistePorLegajoAsync(dto.Legajo, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese legajo.");

        if (await usuarioRepository.ExistePorEmailAsync(dto.Email, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var usuario = Usuario.Crear(dto.DNI, dto.Legajo, dto.Email, dto.Nombre, dto.Apellido, passwordHash, rol);

        await usuarioRepository.AgregarAsync(usuario, cancellationToken);
    }
}
