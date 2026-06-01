namespace PracticaProfesional.Application.Reportes.DTOs;

public class ResultadoPreguntaDto
{
    public int     PreguntaId   { get; init; }
    public string  TextoPregunta { get; init; } = string.Empty;
    public int     TotalRespuestas { get; init; }
    public decimal? PromedioLikert { get; init; }
    public List<string> TextosLibres { get; init; } = [];
}

// ── RR-03: Satisfacción Estudiantil ──────────────────────────────────────────

public class PuntoSatisfaccionDto
{
    public string  Periodo         { get; init; } = string.Empty;
    public int     TotalRespuestas { get; init; }
    public decimal? PromedioGeneral { get; init; }
}

public class ReporteSatisfaccionDto
{
    public int    EncuestaId    { get; init; }
    public string EncuestaTitulo { get; init; } = string.Empty;
    public int    TotalRespuestas { get; init; }
    public decimal? PromedioGlobal { get; init; }
    public List<ResultadoPreguntaDto>  ResultadosPorPregunta { get; init; } = [];
    public List<PuntoSatisfaccionDto>  EvolucionMensual      { get; init; } = [];
    public DateTime GeneradoEn { get; init; }
}

// ── RR-04: Comparativo Institucional ─────────────────────────────────────────

public class ReporteComparativoEncuestasDto
{
    public DateTime GeneradoEn { get; init; }
    public List<FilaComparativoEncuestaDto> Encuestas { get; init; } = [];
}

public class FilaComparativoEncuestaDto
{
    public int     EncuestaId     { get; init; }
    public string  Titulo         { get; init; } = string.Empty;
    public string  Tipo           { get; init; } = string.Empty;
    public int     CicloLectivo   { get; init; }
    public int     TotalRespuestas { get; init; }
    public decimal? PromedioGeneral { get; init; }
}
