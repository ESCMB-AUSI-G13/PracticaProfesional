using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Encuestas.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Application.Encuestas;

public class ListarEncuestasDocenteUseCase(
    IDocenteRepository           docenteRepo,
    IEspacioCurricularRepository espacioRepo,
    AppDbContext                 db)
{
    public async Task<List<EncuestaDto>> EjecutarAsync(int usuarioId, CancellationToken ct = default)
    {
        var materiaIds = await ObtenerMateriaIdsAsync(usuarioId, ct);
        if (materiaIds.Count == 0) return [];

        var encuestas = await db.Encuestas
            .Include(e => e.Preguntas.OrderBy(p => p.Orden))
            .Include(e => e.Materia)
            .Where(e => e.Tipo == TipoEncuesta.EvaluacionDocente
                     && e.MateriaId != null
                     && materiaIds.Contains(e.MateriaId.Value))
            .OrderByDescending(e => e.CicloLectivo)
            .ThenBy(e => e.Titulo)
            .ToListAsync(ct);

        return encuestas.Select(ListarEncuestasUseCase.ToDto).ToList();
    }

    public async Task<List<MateriaEncuestaDto>> ObtenerMateriasAsync(int usuarioId, CancellationToken ct = default)
    {
        var docente = await docenteRepo.ObtenerPorUsuarioIdAsync(usuarioId, ct);
        if (docente is null) return [];

        var espacios = await espacioRepo.ListarPorDocenteIdAsync(docente.Id, ct);

        return espacios
            .Select(e => e.Materia)
            .DistinctBy(m => m.Id)
            .OrderBy(m => m.Nombre)
            .Select(m => new MateriaEncuestaDto(m.Id, m.Nombre, m.Codigo))
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
