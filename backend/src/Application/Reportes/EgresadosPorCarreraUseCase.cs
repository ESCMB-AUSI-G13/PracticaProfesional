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
        var duracionesValidas = filas
            .Where(f => f.DuracionPromedioAnios.HasValue)
            .Select(f => f.DuracionPromedioAnios!.Value)
            .ToList();

        return new ReporteEgresadosPorCarreraDto
        {
            Filas                  = filas,
            PorCarrera             = porCarrera,
            TotalGeneral           = totalEgresados,
            TasaEgresoGlobal       = totalAlumnos > 0
                ? Math.Round((double)totalEgresados / totalAlumnos * 100, 1)
                : 0,
            DuracionPromedioGlobal = duracionesValidas.Any()
                ? Math.Round(duracionesValidas.Average(), 1)
                : null,
        };
    }
}
