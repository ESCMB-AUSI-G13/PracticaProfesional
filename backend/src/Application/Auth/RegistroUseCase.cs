using PracticaProfesional.Application.Auth.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Auth;

public class RegistroUseCase(
    IUsuarioRepository usuarioRepository,
    IEstudianteRepository estudianteRepository,
    ICarreraRepository carreraRepository,
    IPadronRepository padronRepository,
    IUnitOfWork unitOfWork)
{
    public async Task EjecutarAsync(RegistroRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (!await padronRepository.ExisteDniAsync(dto.DNI.Trim(), cancellationToken))
            throw new BusinessException("El DNI no está habilitado para el registro. Comunicate con la dirección del instituto.", 403);

        if (await usuarioRepository.ExistePorDniAsync(dto.DNI, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese DNI.");

        if (await usuarioRepository.ExistePorEmailAsync(dto.Email, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        _ = await carreraRepository.ObtenerPorIdAsync(dto.CarreraId, cancellationToken)
            ?? throw new BusinessException($"No se encontró la carrera con Id {dto.CarreraId}.");

        Usuario.ValidarFortalezaPassword(dto.Password);

        var legajo = await usuarioRepository.GenerarProximoLegajoAsync(cancellationToken);
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var usuario = Usuario.Crear(dto.DNI, legajo, dto.Email, dto.Nombre, dto.Apellido, passwordHash, Rol.Estudiante);

        // Usuario + Estudiante + consumo del Padrón deben confirmarse juntos: sin transacción,
        // un fallo a mitad de camino dejaba un Usuario huérfano o el DNI liberado para un
        // segundo registro antes de que el primero tuviera perfil (ver CHECKLIST.md, Tier 2 #4).
        await unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            await usuarioRepository.AgregarAsync(usuario, cancellationToken);

            var estudiante = Estudiante.Crear(usuario.Id, dto.Anio, dto.CarreraId, dto.FechaDeIngreso);
            await estudianteRepository.AgregarAsync(estudiante, cancellationToken);

            await padronRepository.EliminarAsync(dto.DNI.Trim(), cancellationToken);
        }, cancellationToken: cancellationToken);
    }
}
