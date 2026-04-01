using PracticaProfesional.Application.Estudiantes.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Estudiantes;

public class ModificarEstudianteUseCase(
    IUsuarioRepository usuarioRepository,
    IEstudianteRepository estudianteRepository)
{
    public async Task<EstudianteDto> EjecutarAsync(int usuarioId, ModificarEstudianteDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Estudiante no encontrado.");

        var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Perfil de estudiante no encontrado.");

        if (await usuarioRepository.ExistePorEmailExcluyendoIdAsync(dto.Email, usuarioId, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        if (!Enum.TryParse<CondicionEstudiante>(dto.Condicion, ignoreCase: true, out var condicion))
            throw new ArgumentException($"Condición inválida: {dto.Condicion}");

        usuario.Modificar(dto.Nombre, dto.Apellido, dto.Email, usuario.Rol);
        estudiante.Modificar(dto.Anio, condicion);

        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        return CrearEstudianteUseCase.ToDto(estudiante, usuario);
    }
}
