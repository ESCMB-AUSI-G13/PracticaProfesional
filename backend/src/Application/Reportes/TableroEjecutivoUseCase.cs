using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes.DTOs;

namespace PracticaProfesional.Application.Reportes;

public class TableroEjecutivoUseCase(IRendimientoConsolidadoRepository repo)
{
    // Mismos umbrales que RiesgoAcademicoUseCase
    private const decimal UmbralAsistMedio    = 20m;
    private const decimal UmbralAsistAlto     = 35m;
    private const decimal UmbralNotaAprobado  = 4m;
    private const decimal UmbralNotaSuficiente = 6m;

    public async Task<TableroEjecutivoDto> EjecutarAsync(CancellationToken ct = default)
    {
        var datosRiesgo      = await repo.ObtenerDatosRiesgoAsync(null, null, ct);
        var datosCohorte     = await repo.ObtenerDatosCohorteAsync(null, null, ct);
        var datosCatedras    = await repo.ObtenerPromediosCatedraAsync(null, null, null, ct);
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

        // ── Riesgo académico ─────────────────────────────────────────────────
        int alto = 0, medio = 0, bajo = 0;

        foreach (var d in datosRiesgo)
        {
            decimal pct = d.TotalClases > 0
                ? Math.Round((decimal)d.Ausencias / d.TotalClases * 100, 1) : 0m;

            var nivel = CalcularNivel(d.Condicion, pct, d.PromedioNotas, d.Reprobadas);
            if (nivel == "Alto")       alto++;
            else if (nivel == "Medio") medio++;
            else                       bajo++;
        }

        int totalActivos = alto + medio + bajo;
        decimal pctAlto  = totalActivos > 0
            ? Math.Round((decimal)alto / totalActivos * 100, 1) : 0m;

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
            RiesgoAlto                 = alto,
            RiesgoMedio                = medio,
            RiesgoBajo                 = bajo,
            PorcentajeRiesgoAlto       = pctAlto,
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

    private static string CalcularNivel(
        string condicion, decimal pctInasistencias, decimal? promedio, int reprobadas)
    {
        if (condicion == "Libre") return "Alto";

        var dimA = pctInasistencias > UmbralAsistAlto   ? "Alto"  :
                   pctInasistencias >= UmbralAsistMedio ? "Medio" : "Bajo";

        string dimB;
        if (promedio is null)
            dimB = "Bajo";
        else if (promedio < UmbralNotaAprobado || reprobadas >= 2)
            dimB = "Alto";
        else if (promedio < UmbralNotaSuficiente || reprobadas == 1)
            dimB = "Medio";
        else
            dimB = "Bajo";

        if (dimA == "Alto"  || dimB == "Alto")  return "Alto";
        if (dimA == "Medio" || dimB == "Medio") return "Medio";
        return "Bajo";
    }
}
