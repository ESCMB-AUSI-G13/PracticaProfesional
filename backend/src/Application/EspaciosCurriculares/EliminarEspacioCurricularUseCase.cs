using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.EspaciosCurriculares;

public class EliminarEspacioCurricularUseCase(
    IEspacioCurricularRepository repository,
    IAuditoriaService auditoria)
{
    public async Task EjecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var ec = await repository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró la cátedra con Id {id}.");

        await auditoria.RegistrarAsync("EspacioCurricular", ec.Id.ToString(), "ELIMINAR",
            valorAnterior: new { ec.MateriaId, ec.DocenteId, ec.CursoId },
            valorNuevo: null,
            cancellationToken: cancellationToken);

        await repository.EliminarAsync(ec, cancellationToken);
    }
}
