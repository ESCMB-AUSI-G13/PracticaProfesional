using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Estudiantes;

public class EliminarEstudianteUseCase(
    IUsuarioRepository usuarioRepository,
    IEstudianteRepository estudianteRepository,
    IAuditoriaService auditoria)
{
    public async Task EjecutarAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Estudiante no encontrado.");

        if (usuario.Rol != Rol.Estudiante)
            throw new InvalidOperationException("El usuario no es un estudiante.");

        var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Datos del estudiante no encontrados.");

        await auditoria.RegistrarAsync("Estudiante", usuarioId.ToString(), "ELIMINAR",
            valorAnterior: new { usuario.DNI, usuario.Legajo, usuario.Email, usuario.Nombre, usuario.Apellido },
            valorNuevo: null,
            cancellationToken);

        await estudianteRepository.EliminarAsync(estudiante.Id, usuarioId, cancellationToken);
    }
}
