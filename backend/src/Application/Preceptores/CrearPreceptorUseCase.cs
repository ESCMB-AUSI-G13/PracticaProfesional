using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Preceptores.DTOs;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Preceptores;

public class CrearPreceptorUseCase(
    IUsuarioRepository usuarioRepository,
    IPreceptorRepository preceptorRepository,
    IAuditoriaService auditoria)
{
    public async Task<PreceptorDto> EjecutarAsync(CrearPreceptorDto dto, CancellationToken cancellationToken = default)
    {
        if (await usuarioRepository.ExistePorDniAsync(dto.DNI, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese DNI.");

        if (await usuarioRepository.ExistePorLegajoAsync(dto.Legajo, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese legajo.");

        if (await usuarioRepository.ExistePorEmailAsync(dto.Email, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var usuario = Usuario.Crear(dto.DNI, dto.Legajo, dto.Email, dto.Nombre, dto.Apellido, passwordHash, Rol.Preceptor);

        await usuarioRepository.AgregarAsync(usuario, cancellationToken);

        var preceptor = Preceptor.Crear(usuario.Id, dto.Telefono, dto.Turno);
        await preceptorRepository.AgregarAsync(preceptor, cancellationToken);

        await auditoria.RegistrarAsync("Preceptor", preceptor.Id.ToString(), "CREAR",
            valorAnterior: null,
            valorNuevo: new { usuario.DNI, usuario.Legajo, usuario.Email, usuario.Nombre, usuario.Apellido, preceptor.Telefono, preceptor.Turno },
            cancellationToken);

        return ToDto(preceptor, usuario);
    }

    internal static PreceptorDto ToDto(Preceptor p, Usuario u) => new(
        p.Id, u.Id, u.DNI, u.Legajo, u.Email, u.Nombre, u.Apellido,
        p.Telefono, p.Turno, u.Activo, u.FechaCreacion
    );
}
