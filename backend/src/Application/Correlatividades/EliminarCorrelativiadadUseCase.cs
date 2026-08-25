using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Correlatividades;

public class EliminarCorrelativiadadUseCase(
    ICorrelativiadadRepository correlativiadadRepository,
    IAuditoriaService auditoria)
{
    public async Task EjecutarAsync(int id, CancellationToken cancellationToken = default)
    {
        var correlatividad = await correlativiadadRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró la correlatividad con Id {id}.");

        await correlativiadadRepository.EliminarAsync(correlatividad, cancellationToken);

        // Ver CHECKLIST.md, Tier 5 #25 — mismo motivo que en la creación.
        await auditoria.RegistrarAsync("Correlatividad", id.ToString(), "ELIMINAR",
            valorAnterior: new { correlatividad.MateriaDestinoId, correlatividad.MateriaRequisitoId, correlatividad.TipoRequerimiento, CondicionAcademica = correlatividad.CondicionAcademica.ToString() },
            valorNuevo: null,
            cancellationToken: cancellationToken);
    }
}
