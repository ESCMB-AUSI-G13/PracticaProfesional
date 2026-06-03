using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes.DTOs;

namespace PracticaProfesional.Application.Reportes;

public class TableroEjecutivoUseCase(IRendimientoConsolidadoRepository repo)
{
    public async Task<TableroEjecutivoDto> EjecutarAsync(CancellationToken ct = default)
    {
        var datosActivos     = await repo.ObtenerDatosRiesgoAsync(null, null, ct);
        var datosCohorte     = await repo.ObtenerDatosCohorteAsync(null, null, ct);
        var datosCatedras    = await repo.ObtenerPromediosCatedraAsync(null, null, null, null, ct);
        var evolucionMatric  = await repo.ObtenerEvolucionMatriculaAsync(ct);

        // ── Matrícula histórica ───────────────────────────────────────────────
        int totalHistorico  = datosCohorte.Sum(c => c.Total);
        int totalMatriculados = datosCohorte.Sum(c => c.Activos);
        int totalEgresados  = datosCohorte.Sum(c => c.Egresados);
        int totalDesertores = datosCohorte.Sum(c => c.Desertores);

        decimal tasaDesercion = totalHistorico > 0
            ? Math.Round((decimal)totalDesertores / totalHistorico * 100, 1) : 0m;
        decimal tasaEgreso = totalHistorico > 0
            ? Math.Round((decimal)totalEgresados / totalHistorico * 100, 1) : 0m;
        // Retención = activos + egresados (ambos grupos permanecieron vinculados al sistema).
        decimal tasaRetencion = totalHistorico > 0
            ? Math.Round((decimal)(totalMatriculados + totalEgresados) / totalHistorico * 100, 1) : 0m;

        // ── Condición académica (activos) ─────────────────────────────────────
        int promocionales = datosActivos.Count(d => d.Condicion == "Promocional");
        int regulares     = datosActivos.Count(d => d.Condicion == "Regular");
        int libres        = datosActivos.Count(d => d.Condicion == "Libre");

        // ── Rendimiento global ───────────────────────────────────────────────
        var catedrasList = datosCatedras.ToList();

        int totalConNota    = catedrasList.Sum(c => c.TotalConNota);
        int totalAprobados  = catedrasList.Sum(c => c.Aprobados);

        decimal pctAprobacion = totalConNota > 0
            ? Math.Round((decimal)totalAprobados / totalConNota * 100, 1) : 0m;

        decimal? promedioGlobal = null;
        var promediosValidos = catedrasList
            .Where(c => c.PromedioGeneral.HasValue && c.TotalConNota > 0)
            .ToList();

        if (promediosValidos.Count > 0)
        {
            double sumaPonderada = promediosValidos.Sum(c => (double)c.PromedioGeneral!.Value * c.TotalConNota);
            int    pesoTotal     = promediosValidos.Sum(c => c.TotalConNota);
            promedioGlobal = pesoTotal > 0
                ? Math.Round((decimal)(sumaPonderada / pesoTotal), 2) : null;
        }

        // ── Evolución por cohorte ────────────────────────────────────────────
        var evolucion = datosCohorte
            .GroupBy(c => c.AnioCohorte)
            .Select(g => new EvolucionCohorteResumenDto
            {
                AnioCohorte = g.Key,
                Total       = g.Sum(c => c.Total),
                Activos     = g.Sum(c => c.Activos),
                Egresados   = g.Sum(c => c.Egresados),
                Desertores  = g.Sum(c => c.Desertores),
            })
            .OrderBy(e => e.AnioCohorte)
            .ToList();

        return new TableroEjecutivoDto
        {
            TotalMatriculados          = totalMatriculados,
            TotalEgresados             = totalEgresados,
            TotalDesertores            = totalDesertores,
            TotalHistorico             = totalHistorico,
            Promocionales              = promocionales,
            Regulares                  = regulares,
            Libres                     = libres,
            TasaRetencionGlobal        = tasaRetencion,
            TasaDesercionGlobal        = tasaDesercion,
            TasaEgresoGlobal           = tasaEgreso,
            PromedioNotaGlobal         = promedioGlobal,
            PorcentajeAprobacionGlobal = pctAprobacion,
            EvolucionCohortes          = evolucion,
            EvolucionMatricula         = evolucionMatric,
            GeneradoEn                 = DateTime.UtcNow,
        };
    }

}
