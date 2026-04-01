using PracticaProfesional.Application.Docentes.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Docentes;

public class CrearDocenteUseCase(
    IUsuarioRepository usuarioRepository,
    IDocenteRepository docenteRepository)
{
    public async Task<DocenteDto> EjecutarAsync(CrearDocenteDto dto, CancellationToken cancellationToken = default)
    {
        if (await usuarioRepository.ExistePorDniAsync(dto.DNI, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese DNI.");

        if (await usuarioRepository.ExistePorLegajoAsync(dto.Legajo, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese legajo.");

        if (await usuarioRepository.ExistePorEmailAsync(dto.Email, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var usuario = Usuario.Crear(dto.DNI, dto.Legajo, dto.Email, dto.Nombre, dto.Apellido, passwordHash, Rol.Docente);

        await usuarioRepository.AgregarAsync(usuario, cancellationToken);

        var docente = Docente.Crear(usuario.Id, dto.Telefono, dto.Categoria);
        await docenteRepository.AgregarAsync(docente, cancellationToken);

        return ToDto(docente, usuario);
    }

    internal static DocenteDto ToDto(Docente d, Usuario u) => new(
        d.Id, u.Id, u.DNI, u.Legajo, u.Email, u.Nombre, u.Apellido,
        d.Telefono, d.Categoria, u.Activo, u.FechaCreacion
    );
}
