using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes.DTOs;

namespace PracticaProfesional.Application.Reportes;

public class RetencionAnualUseCase(IRendimientoConsolidadoRepository repo)
{
    private const decimal UmbralAlerta  = 85m;
    private const int     MaxAniosPlan  = 5;

    public async Task<ReporteRetencionAnualDto> EjecutarAsync(
        int? carreraId, int? anioCohorte, CancellationToken ct = default)
    {
        var raw = await repo.ObtenerDatosRetencionAnualAsync(carreraId, anioCohorte, ct);

        if (raw.Count == 0)
            return new ReporteRetencionAnualDto { UmbralAlerta = UmbralAlerta };

        int anioActual = DateTime.UtcNow.Year;

        var grupos = raw
            .GroupBy(r => new { r.AnioCohorte, r.Carrera })
            .OrderBy(g => g.Key.AnioCohorte)
            .ThenBy(g => g.Key.Carrera);

        var cohortes = new List<CohorteRetencionAnualDto>();

        foreach (var g in grupos)
        {
            int totalInicial = g.Select(r => r.EstudianteId).Distinct().Count();

            var tasas = new Dictionary<int, decimal>();

            for (int n = 1; n <= MaxAniosPlan; n++)
            {
                int calendarioAnio = g.Key.AnioCohorte + (n - 1);

                // No proyectar años que todavía no transcurrieron
                if (calendarioAnio > anioActual) break;

                if (n == 1)
                {
                    // El año de ingreso siempre es la base → 100 %
                    tasas[1] = 100m;
                }
                else
                {
                    // Para el año actual: excluir desertores (se inscribieron pero ya dejaron)
                    // Para años pasados: contar a todos los que tuvieron actividad (histórico)
                    var registrosAnio = g.Where(r => r.AnioHistorial == calendarioAnio);
                    if (calendarioAnio == anioActual)
                        registrosAnio = registrosAnio.Where(r => !r.EsDesertor);

                    int count = registrosAnio.Select(r => r.EstudianteId).Distinct().Count();
                    tasas[n] = totalInicial > 0
                        ? Math.Round((decimal)count / totalInicial * 100, 1)
                        : 0m;
                }
            }

            cohortes.Add(new CohorteRetencionAnualDto
            {
                AnioCohorte  = g.Key.AnioCohorte,
                Carrera      = g.Key.Carrera,
                TotalInicial = totalInicial,
                TasasPorAnio = tasas
            });
        }

        int maxAnios = cohortes.Count > 0
            ? cohortes.Max(c => c.TasasPorAnio.Keys.DefaultIfEmpty(1).Max())
            : 1;

        // Promedio de cada año ordinal sobre las cohortes que tienen dato para ese año
        var promedios = new Dictionary<int, decimal>();
        for (int n = 1; n <= maxAnios; n++)
        {
            var valores = cohortes
                .Where(c => c.TasasPorAnio.ContainsKey(n))
                .Select(c => c.TasasPorAnio[n])
                .ToList();

            if (valores.Count > 0)
                promedios[n] = Math.Round(valores.Average(), 1);
        }

        return new ReporteRetencionAnualDto
        {
            Cohortes         = cohortes,
            PromediosPorAnio = promedios,
            MaxAnios         = maxAnios,
            UmbralAlerta     = UmbralAlerta
        };
    }
}
