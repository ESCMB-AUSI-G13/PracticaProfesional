using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Cursos;

public class CerrarCursoUseCase(ICursoRepository cursoRepository, IAuditoriaService auditoria)
{
    public async Task EjecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var curso = await cursoRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró el curso con Id {id}.");

        curso.Cerrar();
        await cursoRepository.GuardarCambiosAsync(cancellationToken);

        await auditoria.RegistrarAsync("Curso", curso.Id.ToString(), "CERRAR",
            valorAnterior: new { Estado = "Activo" },
            valorNuevo: new { Estado = "Cerrado" },
            cancellationToken: cancellationToken);
    }
}
