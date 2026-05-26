using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes.DTOs;

namespace PracticaProfesional.Application.Reportes;

public class RiesgoAcademicoUseCase(IRendimientoConsolidadoRepository repo)
{
    // Umbrales DimA — asistencia (régimen académico)
    // < 20% inasist → Bajo (zona Promocional)
    // 20–35%        → Medio (zona Regular)
    // > 35%         → Alto (zona Libre)
    private const decimal UmbralAsistMedio = 20m;
    private const decimal UmbralAsistAlto  = 35m;

    // Umbrales DimB — rendimiento
    private const decimal UmbralNotaAprobado  = 4m;
    private const decimal UmbralNotaSuficiente = 6m;

    public async Task<ReporteRiesgoAcademicoDto> EjecutarAsync(
        FiltroRiesgoAcademicoDto filtro,
        CancellationToken ct = default)
    {
        var datos = await repo.ObtenerDatosRiesgoAsync(filtro.AnioCohorte, filtro.CarreraId, ct);

        var resultado = datos.Select(d =>
        {
            decimal pctInasistencias = d.TotalClases > 0
                ? Math.Round((decimal)d.Ausencias / d.TotalClases * 100, 1)
                : 0m;

            var nivel = CalcularNivel(d.Condicion, pctInasistencias, d.PromedioNotas, d.Reprobadas);

            return new RiesgoAcademicoDto
            {
                EstudianteId            = d.EstudianteId,
                Legajo                  = d.Legajo,
                NombreCompleto          = $"{d.Apellido}, {d.Nombre}",
                Carrera                 = d.Carrera,
                AnioCarrera             = d.AnioCarrera,
                AnioCohorte             = d.AnioCohorte,
                Condicion               = d.Condicion,
                NivelRiesgo             = nivel,
                PorcentajeInasistencias = pctInasistencias,
                PromedioNotas           = d.PromedioNotas.HasValue
                                          ? Math.Round(d.PromedioNotas.Value, 2)
                                          : null,
                MateriasReprobadas      = d.Reprobadas
            };
        })
        .Where(r => filtro.NivelRiesgo is null
                 || r.NivelRiesgo.Equals(filtro.NivelRiesgo, StringComparison.OrdinalIgnoreCase))
        .OrderBy(r => r.NivelRiesgo == "Alto"  ? 0 :
                      r.NivelRiesgo == "Medio" ? 1 : 2)
        .ThenBy(r => r.NombreCompleto)
        .ToList();

        return new ReporteRiesgoAcademicoDto
        {
            Estudiantes = resultado,
            TotalAlto   = resultado.Count(r => r.NivelRiesgo == "Alto"),
            TotalMedio  = resultado.Count(r => r.NivelRiesgo == "Medio"),
            TotalBajo   = resultado.Count(r => r.NivelRiesgo == "Bajo")
        };
    }

    private static string CalcularNivel(
        string condicion,
        decimal pctInasistencias,
        decimal? promedio,
        int reprobadas)
    {
        // Libre = ya perdió regularidad → riesgo máximo siempre
        if (condicion == "Libre") return "Alto";

        // DimA — asistencia
        var dimA = pctInasistencias > UmbralAsistAlto   ? "Alto"  :
                   pctInasistencias >= UmbralAsistMedio ? "Medio" : "Bajo";

        // DimB — rendimiento (sin notas → no penaliza)
        string dimB;
        if (promedio is null)
            dimB = "Bajo";
        else if (promedio < UmbralNotaAprobado || reprobadas >= 2)
            dimB = "Alto";
        else if (promedio < UmbralNotaSuficiente || reprobadas == 1)
            dimB = "Medio";
        else
            dimB = "Bajo";

        // Nivel final = el peor de las dos dimensiones
        if (dimA == "Alto"  || dimB == "Alto")  return "Alto";
        if (dimA == "Medio" || dimB == "Medio") return "Medio";
        return "Bajo";
    }
}
