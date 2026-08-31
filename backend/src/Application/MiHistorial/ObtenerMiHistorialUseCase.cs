using PracticaProfesional.Application.MiHistorial.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.MiHistorial;

/// <summary>
/// CU-43: el estudiante consulta su propio historial académico, desglosado por materia con
/// cada parcial rendido y la nota final, más el promedio general (de las materias con nota
/// final registrada).
/// </summary>
public class ObtenerMiHistorialUseCase(
    IEstudianteRepository estudianteRepository,
    IHistorialAcademicoRepository historialRepository,
    IInscripcionExamenRepository inscripcionExamenRepository)
{
    public async Task<MiHistorialDto> EjecutarAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el perfil de estudiante para el usuario {usuarioId}.");

        var historial = await historialRepository.ObtenerConMateriaPorEstudianteAsync(estudiante.Id, cancellationToken);

        // Última cursada por materia (una materia puede tener más de un registro si se
        // recursó tras quedar Libre).
        var ultimoHistorialPorMateria = historial
            .GroupBy(h => h.MateriaId)
            .Select(g => g.OrderByDescending(h => h.Anio).First())
            .ToDictionary(h => h.MateriaId);

        var misExamenes = await inscripcionExamenRepository.ListarPorEstudianteAsync(estudiante.Id, cancellationToken);
        var parcialesPorMateria = misExamenes
            .Where(i => i.Examen.TipoExamen == TipoExamen.Parcial && i.Estado != EstadoInscripcion.Baja)
            .GroupBy(i => i.Examen.MateriaId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(i => i.Examen.FechaExamen)
                    .Select(i => new ParcialDto(
                        i.Examen.FechaExamen.ToString("yyyy-MM-dd"),
                        i.NotaValor,
                        i.Estado.ToString()))
                    .ToList());

        // Unión: materias con cursada registrada + materias con algún parcial rendido
        // (por si en algún momento hay parciales sin cierre de curso todavía).
        var materiaIds = ultimoHistorialPorMateria.Keys.Concat(parcialesPorMateria.Keys).Distinct();

        var materias = materiaIds.Select(materiaId =>
        {
            ultimoHistorialPorMateria.TryGetValue(materiaId, out var h);
            parcialesPorMateria.TryGetValue(materiaId, out var parciales);

            var materiaCodigo = h?.Materia.Codigo ?? misExamenes.First(i => i.Examen.MateriaId == materiaId).Examen.Materia.Codigo;
            var materiaNombre = h?.Materia.Nombre ?? misExamenes.First(i => i.Examen.MateriaId == materiaId).Examen.Materia.Nombre;

            return new HistorialMateriaDto(
                materiaId,
                materiaCodigo,
                materiaNombre,
                parciales ?? [],
                h?.NotaFinal,
                h?.EstadoFinal,
                h?.Condicion.ToString(),
                h?.Anio
            );
        }).OrderBy(m => m.MateriaCodigo).ToList();

        var notasFinales = materias.Where(m => m.NotaFinal.HasValue).Select(m => m.NotaFinal!.Value).ToList();
        decimal? promedio = notasFinales.Count > 0 ? Math.Round(notasFinales.Average(), 2) : null;

        return new MiHistorialDto(materias, promedio);
    }
}
