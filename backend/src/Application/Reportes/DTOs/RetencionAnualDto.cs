namespace PracticaProfesional.Application.Reportes.DTOs;

public class CohorteRetencionAnualDto
{
    public int    AnioCohorte   { get; init; }
    public string Carrera       { get; init; } = string.Empty;
    public int    TotalInicial  { get; init; }

    // Clave = año ordinal (1, 2, 3, 4, 5).  Valor = tasa de retención (0–100).
    // Solo contiene entradas hasta el año que ya transcurrió; años futuros se omiten.
    public Dictionary<int, decimal> TasasPorAnio { get; init; } = [];
}

public class ReporteRetencionAnualDto
{
    public List<CohorteRetencionAnualDto> Cohortes        { get; init; } = [];

    // Promedio de la tasa de cada año ordinal a través de todas las cohortes con dato.
    public Dictionary<int, decimal> PromediosPorAnio      { get; init; } = [];

    // Máximo de años ordinales con datos disponibles (determina cuántas columnas mostrar).
    public int     MaxAnios      { get; init; }
    public decimal UmbralAlerta  { get; init; } = 85m;
}
