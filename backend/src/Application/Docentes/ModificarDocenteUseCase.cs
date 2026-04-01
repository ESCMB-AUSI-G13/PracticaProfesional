using PracticaProfesional.Application.Docentes.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Docentes;

public class ModificarDocenteUseCase(
    IUsuarioRepository usuarioRepository,
    IDocenteRepository docenteRepository)
{
    public async Task<DocenteDto> EjecutarAsync(int usuarioId, ModificarDocenteDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Docente no encontrado.");

        var docente = await docenteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Perfil de docente no encontrado.");

        if (await usuarioRepository.ExistePorEmailExcluyendoIdAsync(dto.Email, usuarioId, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        usuario.Modificar(dto.Nombre, dto.Apellido, dto.Email, usuario.Rol);
        docente.Modificar(dto.Telefono, dto.Categoria);

        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        return CrearDocenteUseCase.ToDto(docente, usuario);
    }
}
