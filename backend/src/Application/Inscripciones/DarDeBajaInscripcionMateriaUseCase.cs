using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Inscripciones;

public class DarDeBajaInscripcionMateriaUseCase(
    IInscripcionMateriaRepository repository,
    IAuditoriaService auditoria)
{
    /// <param name="usuarioIdSolicitante">Usuario autenticado que pide la baja.</param>
    /// <param name="esStaff">true si es Direccion o Preceptor (pueden dar de baja cualquier
    /// inscripción); si es false (Estudiante), solo puede dar de baja la propia — ver
    /// CHECKLIST.md, Tier 5 #21.</param>
    public async Task EjecutarAsync(
        int id, int usuarioIdSolicitante, bool esStaff, CancellationToken cancellationToken = default)
    {
        var inscripcion = await repository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró la inscripción con Id {id}.");

        if (!esStaff && inscripcion.Estudiante.UsuarioId != usuarioIdSolicitante)
            throw new BusinessException("No podés dar de baja la inscripción de otro estudiante.", 403);

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
