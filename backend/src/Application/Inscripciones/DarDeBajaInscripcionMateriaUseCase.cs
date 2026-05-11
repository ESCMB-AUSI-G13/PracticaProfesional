using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Inscripciones;

public class DarDeBajaInscripcionMateriaUseCase(
    IInscripcionMateriaRepository repository,
    IAuditoriaService auditoria)
{
    public async Task EjecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var inscripcion = await repository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró la inscripción con Id {id}.");

        if (inscripcion.Estado != EstadoInscripcion.Activa)
            throw new BusinessException("Solo se puede dar de baja una inscripción activa.");

        inscripcion.DarDeBaja();

        await auditoria.RegistrarAsync("InscripcionMateria", id.ToString(), "BAJA",
            valorAnterior: new { inscripcion.EstudianteId, inscripcion.MateriaId, Estado = "Activa" },
            valorNuevo:    new { inscripcion.EstudianteId, inscripcion.MateriaId, Estado = "Baja" },
            cancellationToken: cancellationToken);

        await repository.GuardarCambiosAsync(cancellationToken);
    }
}
