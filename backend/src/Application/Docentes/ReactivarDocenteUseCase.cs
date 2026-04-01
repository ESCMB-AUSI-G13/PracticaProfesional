using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Docentes;

public class ReactivarDocenteUseCase(IUsuarioRepository usuarioRepository)
{
    public async Task EjecutarAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Docente no encontrado.");

        if (usuario.Rol != Rol.Docente)
            throw new InvalidOperationException("El usuario no es un docente.");

        usuario.Reactivar();
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);
    }
}
