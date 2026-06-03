using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes.DTOs;

namespace PracticaProfesional.Application.Reportes;

public class DesercionPorAnioUseCase(IRendimientoConsolidadoRepository repo)
{
    // Umbral Alto  > 15 % de deserción en ese año
    // Umbral Medio > 8 %
    private const decimal UmbralAlto  = 15m;
    private const decimal UmbralMedio = 8m;

    public async Task<ReporteDesercionPorAnioDto> EjecutarAsync(
        int? carreraId, int? anioCohorte, CancellationToken ct = default)
    {
        var (raw, totalGlobal, desertoresGlobal) =
            await repo.ObtenerDesercionPorAnioAsync(carreraId, anioCohorte, ct);

        var filas = raw
            .OrderBy(r => r.AnioCursada)
            .Select(r =>
            {
                decimal tasa = r.Total > 0
                    ? Math.Round((decimal)r.Desertores / r.Total * 100, 1)
                    : 0m;

                string nivel = tasa > UmbralAlto  ? "Alto"  :
                               tasa > UmbralMedio ? "Medio" : "Bajo";

                return new DesercionPorAnioDto
                {
                    AnioCursada      = r.AnioCursada,
                    TotalEstudiantes = r.Total,
                    Desertores       = r.Desertores,
                    TasaDesercion    = tasa,
                    NivelRiesgo      = nivel
                };
            })
            .ToList();

        decimal global = totalGlobal > 0
            ? Math.Round((decimal)desertoresGlobal / totalGlobal * 100, 1)
            : 0m;

        return new ReporteDesercionPorAnioDto
        {
            Filas            = filas,
            TotalEstudiantes = totalGlobal,
            TotalDesertores  = desertoresGlobal,
            TasaGlobal       = global
        };
    }
}
