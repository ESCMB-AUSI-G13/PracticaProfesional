using PracticaProfesional.Application.Examenes.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Examenes;

public class ListarExamenesUseCase(
    IExamenRepository examenRepository,
    IDocenteRepository docenteRepository,
    IEspacioCurricularRepository espacioCurricularRepository)
{
    /// <summary>
    /// Lista los exámenes. Dirección ve todos; un Docente solo ve los de las
    /// materias en las que tiene un espacio curricular asignado.
    /// </summary>
    public async Task<IEnumerable<ExamenDto>> EjecutarAsync(
        int? docenteUsuarioId,
        CancellationToken cancellationToken = default)
    {
        if (docenteUsuarioId is null)
            return await examenRepository.ListarAsync(cancellationToken: cancellationToken);

        var docente = await docenteRepository.ObtenerPorUsuarioIdAsync(docenteUsuarioId.Value, cancellationToken);
        if (docente is null) return [];

        var espacios = await espacioCurricularRepository.ListarPorDocenteIdAsync(docente.Id, cancellationToken);
        var materiaIds = espacios.Select(e => e.MateriaId).Distinct().ToList();
        if (materiaIds.Count == 0) return [];

        return await examenRepository.ListarAsync(materiaIds, cancellationToken);
    }
}
