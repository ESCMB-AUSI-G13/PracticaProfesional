using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes.DTOs;

namespace PracticaProfesional.Application.Reportes;

/// <summary>
/// RR-06: Evolución de notas en el tiempo.
/// Agrupa las notas por período (año-mes) mostrando promedios y tasas de aprobación.
/// Acceso: Dirección (todos) y Docente (restringido a sus materias).
/// </summary>
public class EvolucionNotasUseCase(IRendimientoConsolidadoRepository repository)
{
    public async Task<ReporteEvolucionNotasDto> EjecutarAsync(
        FiltroEvolucionNotasDto filtro,
        CancellationToken cancellationToken = default)
    {
        string? materia = filtro.MateriaId.HasValue
            ? await repository.ObtenerNombreMateriaAsync(filtro.MateriaId.Value, cancellationToken)
            : null;

        var puntos = await repository.ObtenerEvolucionNotasAsync(
            filtro.MateriaId, filtro.Anio, filtro.DocenteId,
            filtro.Cuatrimestre, filtro.AnioCarrera, filtro.TipoExamen,
            filtro.Granularidad,
            cancellationToken);

        return new ReporteEvolucionNotasDto(
            GeneradoEn:   DateTime.UtcNow,
            MateriaNombre: materia,
            AnioFiltro:   filtro.Anio,
            Evolucion:    puntos);
    }
}
