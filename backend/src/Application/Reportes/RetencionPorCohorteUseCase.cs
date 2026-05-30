using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes.DTOs;

namespace PracticaProfesional.Application.Reportes;

public class RetencionPorCohorteUseCase(IRendimientoConsolidadoRepository repo)
{
    public Task<List<int>> ObtenerAniosAsync(int? carreraId, CancellationToken ct = default)
        => repo.ObtenerAniosCohorteAsync(carreraId, ct);

    public async Task<ReporteRetencionCohorteDto> EjecutarAsync(
        FiltroRetencionCohorteDto filtro,
        CancellationToken ct = default)
    {
        var datos = await repo.ObtenerDatosCohorteAsync(filtro.CarreraId, filtro.AnioCohorte, ct);

        var cohortes = datos.Select(d =>
        {
            // Retención = quienes siguen vinculados (activos + egresados) / total ingresado.
            // Los egresados representan el máximo logro del sistema: se los retiene hasta el título.
            decimal tasaRetencion = d.Total > 0
                ? Math.Round((decimal)(d.Activos + d.Egresados) / d.Total * 100, 1)
                : 0m;

            decimal tasaDesercion = d.Total > 0
                ? Math.Round((decimal)d.Desertores / d.Total * 100, 1)
                : 0m;

            decimal tasaEgreso = d.Total > 0
                ? Math.Round((decimal)d.Egresados / d.Total * 100, 1)
                : 0m;

            return new RetencionCohorteDto
            {
                AnioCohorte   = d.AnioCohorte,
                Carrera       = d.Carrera,
                Total         = d.Total,
                Activos       = d.Activos,
                Egresados     = d.Egresados,
                Desertores    = d.Desertores,
                TasaRetencion = tasaRetencion,
                TasaDesercion = tasaDesercion,
                TasaEgreso    = tasaEgreso
            };
        }).ToList();

        int totalGeneral       = cohortes.Sum(c => c.Total);
        int totalActivos       = cohortes.Sum(c => c.Activos);
        int totalEgresados     = cohortes.Sum(c => c.Egresados);
        int totalDesertores    = cohortes.Sum(c => c.Desertores);

        decimal tasaGlobalRetencion = totalGeneral > 0
            ? Math.Round((decimal)(totalActivos + totalEgresados) / totalGeneral * 100, 1)
            : 0m;

        decimal tasaGlobalDesercion = totalGeneral > 0
            ? Math.Round((decimal)totalDesertores / totalGeneral * 100, 1)
            : 0m;

        return new ReporteRetencionCohorteDto
        {
            Cohortes              = cohortes,
            TotalGeneral          = totalGeneral,
            TasaRetencionGlobal   = tasaGlobalRetencion,
            TasaDesercionGlobal   = tasaGlobalDesercion
        };
    }
}
