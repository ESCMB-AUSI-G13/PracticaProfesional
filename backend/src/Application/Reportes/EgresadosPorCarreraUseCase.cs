using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes.DTOs;

namespace PracticaProfesional.Application.Reportes;

public class EgresadosPorCarreraUseCase(IRendimientoConsolidadoRepository repo)
{
    public async Task<ReporteEgresadosPorCarreraDto> EjecutarAsync(
        int? carreraId, int? anioCohorte, CancellationToken ct = default)
    {
        var raw = await repo.ObtenerEgresadosPorCarreraAsync(carreraId, anioCohorte, ct);

        var filas = raw
            .Select(r => new FilaEgresadoCarreraDto
            {
                Carrera              = r.Carrera,
                AnioCohorte          = r.AnioCohorte,
                TotalEgresados       = r.TotalEgresados,
                TotalAlumnosCohorte  = r.TotalAlumnos,
                TasaEgreso           = r.TotalAlumnos > 0
                    ? Math.Round((double)r.TotalEgresados / r.TotalAlumnos * 100, 1)
                    : 0,
                DuracionPromedioAnios = r.DuracionPromedioAnios.HasValue
                    ? Math.Round(r.DuracionPromedioAnios.Value, 1)
                    : null,
            })
            .ToList();

        var porCarrera = raw
            .GroupBy(r => r.Carrera)
            .Select(g => new ResumenCarreraEgresadosDto
            {
                Carrera = g.Key,
                Total   = g.Sum(r => r.TotalEgresados)
            })
            .OrderByDescending(r => r.Total)
            .ToList();

        int totalEgresados  = filas.Sum(f => f.TotalEgresados);
        int totalAlumnos    = filas.Sum(f => f.TotalAlumnosCohorte);
        var filasConDuracion = filas
            .Where(f => f.DuracionPromedioAnios.HasValue && f.TotalEgresados > 0)
            .ToList();

        double? duracionPonderada = filasConDuracion.Any()
            ? filasConDuracion.Sum(f => f.DuracionPromedioAnios!.Value * f.TotalEgresados)
              / filasConDuracion.Sum(f => f.TotalEgresados)
            : null;

        return new ReporteEgresadosPorCarreraDto
        {
            Filas                  = filas,
            PorCarrera             = porCarrera,
            TotalGeneral           = totalEgresados,
            TasaEgresoGlobal       = totalAlumnos > 0
                ? Math.Round((double)totalEgresados / totalAlumnos * 100, 1)
                : 0,
            DuracionPromedioGlobal = duracionPonderada.HasValue
                ? Math.Round(duracionPonderada.Value, 1)
                : null,
        };
    }
}
