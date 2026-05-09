using PracticaProfesional.Application.Estudiantes.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Estudiantes;

public class CrearEstudianteUseCase(
    IUsuarioRepository usuarioRepository,
    IEstudianteRepository estudianteRepository,
    IAuditoriaService auditoria)
{
    public async Task<EstudianteDto> EjecutarAsync(CrearEstudianteDto dto, CancellationToken cancellationToken = default)
    {
        if (await usuarioRepository.ExistePorDniAsync(dto.DNI, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese DNI.");

        if (await usuarioRepository.ExistePorLegajoAsync(dto.Legajo, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese legajo.");

        if (await usuarioRepository.ExistePorEmailAsync(dto.Email, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var usuario = Usuario.Crear(dto.DNI, dto.Legajo, dto.Email, dto.Nombre, dto.Apellido, passwordHash, Rol.Estudiante);

        await usuarioRepository.AgregarAsync(usuario, cancellationToken);

        var estudiante = Estudiante.Crear(usuario.Id, dto.Anio, dto.Plan, dto.FechaDeIngreso);
        await estudianteRepository.AgregarAsync(estudiante, cancellationToken);

        await auditoria.RegistrarAsync("Estudiante", estudiante.Id.ToString(), "CREAR",
            valorAnterior: null,
            valorNuevo: new { usuario.DNI, usuario.Legajo, usuario.Email, usuario.Nombre, usuario.Apellido, estudiante.Anio, estudiante.Plan, Condicion = estudiante.Condicion.ToString(), estudiante.FechaDeIngreso },
            cancellationToken);

        return ToDto(estudiante, usuario);
    }

    internal static EstudianteDto ToDto(Estudiante e, Usuario u) => new(
        e.Id, u.Id, u.DNI, u.Legajo, u.Email, u.Nombre, u.Apellido,
        e.Anio, e.Plan, e.Condicion.ToString(), e.FechaDeIngreso, u.Activo, u.FechaCreacion
    );
}
