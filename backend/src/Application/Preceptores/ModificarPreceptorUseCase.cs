using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Preceptores.DTOs;

namespace PracticaProfesional.Application.Preceptores;

public class ModificarPreceptorUseCase(
    IUsuarioRepository usuarioRepository,
    IPreceptorRepository preceptorRepository)
{
    public async Task<PreceptorDto> EjecutarAsync(int usuarioId, ModificarPreceptorDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Preceptor no encontrado.");

        var preceptor = await preceptorRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Perfil de preceptor no encontrado.");

        if (await usuarioRepository.ExistePorEmailExcluyendoIdAsync(dto.Email, usuarioId, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        usuario.Modificar(dto.Nombre, dto.Apellido, dto.Email, usuario.Rol);
        preceptor.Modificar(dto.Telefono, dto.Turno);

        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        return CrearPreceptorUseCase.ToDto(preceptor, usuario);
    }
}
