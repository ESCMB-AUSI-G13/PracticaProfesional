using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Usuarios.DTOs;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Usuarios;

public class ListarUsuariosUseCase(IUsuarioRepository usuarioRepository)
{
    public async Task<IEnumerable<UsuarioDto>> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var usuarios = await usuarioRepository.ListarAsync(Rol.Direccion, cancellationToken);
        return usuarios.Select(CrearUsuarioUseCase.ToDto);
    }
}
