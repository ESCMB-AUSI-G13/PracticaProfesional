using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes.DTOs;

namespace PracticaProfesional.Application.Reportes;

/// <summary>
/// RR-07: Promedios por cátedra (EspacioCurricular = Materia + Docente + Curso).
/// Muestra, para cada cátedra, el total de estudiantes, evaluados, aprobados,
/// desaprobados, promedio y porcentaje de aprobación.
/// Acceso: Dirección (todos) y Docente (solo sus cátedras vía DocenteId).
/// </summary>
public class PromediosCatedraUseCase(IRendimientoConsolidadoRepository repository)
{
    public async Task<ReportePromediosCatedraDto> EjecutarAsync(
        FiltroPromediosCatedraDto filtro,
        CancellationToken cancellationToken = default)
    {
        var catedras = await repository.ObtenerPromediosCatedraAsync(
            filtro.DocenteId, filtro.Anio, filtro.CursoId, cancellationToken);

        return new ReportePromediosCatedraDto(
            GeneradoEn: DateTime.UtcNow,
            AnioFiltro: filtro.Anio,
            Catedras:   catedras);
    }
}
