using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Cursos;

public class ReactivarCursoUseCase(ICursoRepository cursoRepository, IAuditoriaService auditoria)
{
    public async Task EjecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var curso = await cursoRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró el curso con Id {id}.");

        var estadoAnterior = curso.Estado.ToString();
        curso.Reactivar();
        await cursoRepository.GuardarCambiosAsync(cancellationToken);

        await auditoria.RegistrarAsync("Curso", curso.Id.ToString(), "REACTIVAR",
            valorAnterior: new { Estado = estadoAnterior },
            valorNuevo: new { Estado = "Activo" },
            cancellationToken: cancellationToken);
    }
}
