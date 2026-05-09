using PracticaProfesional.Application.Auth.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Auth;

public class RegistroUseCase(
    IUsuarioRepository usuarioRepository,
    IEstudianteRepository estudianteRepository,
    ICarreraRepository carreraRepository)
{
    public async Task EjecutarAsync(RegistroRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (await usuarioRepository.ExistePorDniAsync(dto.DNI, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese DNI.");

        if (await usuarioRepository.ExistePorLegajoAsync(dto.Legajo, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese legajo.");

        if (await usuarioRepository.ExistePorEmailAsync(dto.Email, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        _ = await carreraRepository.ObtenerPorIdAsync(dto.CarreraId, cancellationToken)
            ?? throw new BusinessException($"No se encontró la carrera con Id {dto.CarreraId}.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var usuario = Usuario.Crear(dto.DNI, dto.Legajo, dto.Email, dto.Nombre, dto.Apellido, passwordHash, Rol.Estudiante);

        await usuarioRepository.AgregarAsync(usuario, cancellationToken);

        var estudiante = Estudiante.Crear(usuario.Id, dto.Anio, dto.CarreraId, dto.FechaDeIngreso);
        await estudianteRepository.AgregarAsync(estudiante, cancellationToken);
    }
}
