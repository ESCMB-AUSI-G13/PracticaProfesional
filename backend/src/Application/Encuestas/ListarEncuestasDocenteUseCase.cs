using PracticaProfesional.Application.Encuestas.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Encuestas;

public class ListarEncuestasDocenteUseCase(
    IDocenteRepository           docenteRepo,
    IEspacioCurricularRepository espacioRepo,
    IEncuestaRepository          encuestaRepo)
{
    public async Task<List<EncuestaDto>> EjecutarAsync(int usuarioId, CancellationToken ct = default)
    {
        var materiaIds = await ObtenerMateriaIdsAsync(usuarioId, ct);
        if (materiaIds.Count == 0) return [];

        var encuestas = await encuestaRepo.ListarEvaluacionDocentePorMateriasAsync(materiaIds.ToList(), ct);

        return encuestas.Select(ListarEncuestasUseCase.ToDto).ToList();
    }

    public async Task<List<MateriaEncuestaDto>> ObtenerMateriasAsync(int usuarioId, CancellationToken ct = default)
    {
        var docente = await docenteRepo.ObtenerPorUsuarioIdAsync(usuarioId, ct);
        if (docente is null) return [];

        var espacios = await espacioRepo.ListarPorDocenteIdAsync(docente.Id, ct);

        return espacios
            .DistinctBy(e => e.MateriaId)
            .OrderBy(e => e.MateriaNombre)
            .Select(e => new MateriaEncuestaDto(e.MateriaId, e.MateriaNombre, e.MateriaCodigo))
            .ToList();
    }

    public async Task<bool> EsMateriaDelDocenteAsync(int usuarioId, int materiaId, CancellationToken ct = default)
    {
        var ids = await ObtenerMateriaIdsAsync(usuarioId, ct);
        return ids.Contains(materiaId);
    }

    private async Task<HashSet<int>> ObtenerMateriaIdsAsync(int usuarioId, CancellationToken ct)
    {
        var docente = await docenteRepo.ObtenerPorUsuarioIdAsync(usuarioId, ct);
        if (docente is null) return [];

        var espacios = await espacioRepo.ListarPorDocenteIdAsync(docente.Id, ct);
        return espacios.Select(e => e.MateriaId).ToHashSet();
    }
}
