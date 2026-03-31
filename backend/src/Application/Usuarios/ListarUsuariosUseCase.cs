using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Usuarios.DTOs;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Usuarios;

public class ListarUsuariosUseCase(IUsuarioRepository usuarioRepository)
{
    public async Task<IEnumerable<UsuarioDto>> EjecutarAsync(string? rol = null, CancellationToken cancellationToken = default)
    {
        Rol? rolFiltro = null;
        if (!string.IsNullOrWhiteSpace(rol))
        {
            if (!Enum.TryParse<Rol>(rol, ignoreCase: true, out var rolParsed))
                throw new ArgumentException($"Rol inválido: {rol}");
            rolFiltro = rolParsed;
        }

        var usuarios = await usuarioRepository.ListarAsync(rolFiltro, cancellationToken);
        return usuarios.Select(CrearUsuarioUseCase.ToDto);
    }
}
