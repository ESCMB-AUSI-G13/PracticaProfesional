using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class HistorialAcademicoRepository(AppDbContext context) : IHistorialAcademicoRepository
{
    public async Task<IEnumerable<HistorialAcademico>> ObtenerPorEstudianteYMateriaAsync(
        int estudianteId,
        int materiaId,
        CancellationToken cancellationToken = default)
        => await context.HistorialAcademico
            .Where(h => h.EstudianteId == estudianteId && h.MateriaId == materiaId)
            .ToListAsync(cancellationToken);

    /// <summary>
    ///     Regularizado = el estudiante cursó la materia y obtuvo condición Regular o Promocional
    ///     (tiene derecho a rendir el examen final, independientemente de si ya lo rindió).
    /// </summary>
    public async Task<bool> EstaRegularizadoAsync(
        int estudianteId,
        int materiaId,
        CancellationToken cancellationToken = default)
        => await context.HistorialAcademico
            .AnyAsync(h =>
                h.EstudianteId == estudianteId &&
                h.MateriaId == materiaId &&
                (h.Condicion == CondicionEstudiante.Regular || h.Condicion == CondicionEstudiante.Promocional),
            cancellationToken);

    /// <summary>
    ///     Aprobado = el estudiante obtuvo nota final >= 4 en la materia.
    /// </summary>
    public async Task<bool> EstaAprobadoAsync(
        int estudianteId,
        int materiaId,
        CancellationToken cancellationToken = default)
        => await context.HistorialAcademico
            .AnyAsync(h =>
                h.EstudianteId == estudianteId &&
                h.MateriaId == materiaId &&
                h.NotaFinal != null &&
                h.NotaFinal >= 4,
            cancellationToken);

    public async Task<decimal?> ObtenerNotaFinalEnCursoAsync(
        int estudianteId,
        int materiaId,
        int cursoId,
        CancellationToken cancellationToken = default)
        => await context.HistorialAcademico
            .Where(h =>
                h.EstudianteId == estudianteId &&
                h.MateriaId    == materiaId &&
                h.CursoId      == cursoId)
            .Select(h => h.NotaFinal)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<int> ContarAprobadosEnPlanAsync(
        int estudianteId,
        string plan,
        CancellationToken cancellationToken = default)
        => await context.HistorialAcademico
            .Where(h =>
                h.EstudianteId == estudianteId &&
                h.NotaFinal != null &&
                h.NotaFinal >= 4 &&
                h.Materia.Plan == plan)
            .Select(h => h.MateriaId)
            .Distinct()
            .CountAsync(cancellationToken);
}
