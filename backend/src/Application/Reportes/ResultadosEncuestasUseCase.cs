using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes.DTOs;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Reportes;

public class ResultadosEncuestasUseCase(IEncuestaRepository repo)
{
    // ── RR-03 / RR-04: una encuesta → resultados por pregunta + evolución ────
    public async Task<ReporteSatisfaccionDto> ObtenerSatisfaccionAsync(
        int encuestaId, CancellationToken ct = default)
    {
        var encuesta = await repo.ObtenerConRespuestasYPreguntasAsync(encuestaId, ct)
            ?? throw new KeyNotFoundException($"Encuesta {encuestaId} no encontrada.");

        var respuestas = encuesta.Respuestas.ToList();
        int total      = respuestas.Count;

        // Resultados por pregunta
        var porPregunta = encuesta.Preguntas.Select(p =>
        {
            var items = respuestas.SelectMany(r => r.Items)
                                  .Where(i => i.PreguntaId == p.Id)
                                  .ToList();

            var valores = items.Where(i => i.ValorNumerico.HasValue)
                               .Select(i => i.ValorNumerico!.Value)
                               .ToList();

            return new ResultadoPreguntaDto
            {
                PreguntaId      = p.Id,
                TextoPregunta   = p.Texto,
                TotalRespuestas = items.Count,
                PromedioLikert  = p.TipoPregunta == TipoPregunta.EscalaLikert && valores.Count > 0
                    ? Math.Round((decimal)valores.Average(), 2) : null,
                TextosLibres    = p.TipoPregunta == TipoPregunta.TextoLibre
                    ? items.Where(i => !string.IsNullOrWhiteSpace(i.TextoLibre))
                           .Select(i => i.TextoLibre!)
                           .ToList()
                    : []
            };
        }).ToList();

        // Promedio global (solo preguntas Likert)
        var todosValores = respuestas
            .SelectMany(r => r.Items)
            .Where(i => i.ValorNumerico.HasValue)
            .Select(i => i.ValorNumerico!.Value)
            .ToList();

        decimal? promedioGlobal = todosValores.Count > 0
            ? Math.Round((decimal)todosValores.Average(), 2) : null;

        // Evolución mensual
        var evolucion = respuestas
            .GroupBy(r => new { r.Fecha.Year, r.Fecha.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g =>
            {
                var vals = g.SelectMany(r => r.Items)
                            .Where(i => i.ValorNumerico.HasValue)
                            .Select(i => i.ValorNumerico!.Value)
                            .ToList();
                return new PuntoSatisfaccionDto
                {
                    Periodo         = $"{g.Key.Year}-{g.Key.Month:D2}",
                    TotalRespuestas = g.Count(),
                    PromedioGeneral = vals.Count > 0
                        ? Math.Round((decimal)vals.Average(), 2) : null
                };
            }).ToList();

        return new ReporteSatisfaccionDto
        {
            EncuestaId            = encuesta.Id,
            EncuestaTitulo        = encuesta.Titulo,
            TotalRespuestas       = total,
            PromedioGlobal        = promedioGlobal,
            ResultadosPorPregunta = porPregunta,
            EvolucionMensual      = evolucion,
            GeneradoEn            = DateTime.UtcNow
        };
    }

    // ── Docente: comparativo filtrado por sus materias ────────────────────────
    public async Task<ReporteComparativoEncuestasDto> ObtenerComparativoDocenteAsync(
        List<int> materiaIds, CancellationToken ct = default)
    {
        var encuestas = await repo.ListarConRespuestasPorMateriasAsync(materiaIds, ct);
        return BuildComparativo(encuestas);
    }

    // ── RR-04: comparativo entre todas las encuestas ──────────────────────────
    public async Task<ReporteComparativoEncuestasDto> ObtenerComparativoAsync(
        CancellationToken ct = default)
    {
        var encuestas = await repo.ListarTodasConRespuestasAsync(ct);
        return BuildComparativo(encuestas);
    }

    private static ReporteComparativoEncuestasDto BuildComparativo(List<Domain.Entities.Encuesta> encuestas)
    {
        var filas = encuestas.Select(e =>
        {
            var vals = e.Respuestas
                .SelectMany(r => r.Items)
                .Where(i => i.ValorNumerico.HasValue)
                .Select(i => i.ValorNumerico!.Value)
                .ToList();

            return new FilaComparativoEncuestaDto
            {
                EncuestaId      = e.Id,
                Titulo          = e.Titulo,
                Tipo            = e.Tipo.ToString(),
                CicloLectivo    = e.CicloLectivo,
                TotalRespuestas = e.Respuestas.Count,
                PromedioGeneral = vals.Count > 0
                    ? Math.Round((decimal)vals.Average(), 2) : null
            };
        }).ToList();

        return new ReporteComparativoEncuestasDto
        {
            GeneradoEn = DateTime.UtcNow,
            Encuestas  = filas
        };
    }
}
