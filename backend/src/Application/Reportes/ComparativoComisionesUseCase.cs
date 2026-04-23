using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes.DTOs;

namespace PracticaProfesional.Application.Reportes;

/// <summary>
/// RR-05: Comparativo de rendimiento entre comisiones.
/// Muestra, por comisión, la cantidad de inscriptos, evaluados, aprobados,
/// desaprobados, promedio y porcentaje de aprobación.
/// Acceso: Dirección (todos) y Docente (restringido a sus cátedras).
/// </summary>
public class ComparativoComisionesUseCase(IRendimientoConsolidadoRepository repository)
{
    public async Task<ReporteComparativoComisionesDto> EjecutarAsync(
        FiltroComparativoComisionesDto filtro,
        CancellationToken cancellationToken = default)
    {
        string? materia = filtro.MateriaId.HasValue
            ? await repository.ObtenerNombreMateriaAsync(filtro.MateriaId.Value, cancellationToken)
            : null;

        var filas = await repository.ObtenerComparativoComisionesAsync(
            filtro.MateriaId, filtro.Anio, filtro.DocenteId, cancellationToken);

        return new ReporteComparativoComisionesDto(
            GeneradoEn:   DateTime.UtcNow,
            MateriaNombre: materia,
            AnioFiltro:   filtro.Anio,
            Comisiones:   filas);
    }
}
