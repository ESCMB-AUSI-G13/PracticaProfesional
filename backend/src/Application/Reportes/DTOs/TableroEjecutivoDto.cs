namespace PracticaProfesional.Application.Reportes.DTOs;

public class TableroEjecutivoDto
{
    // Matrícula global (histórico total)
    public int TotalMatriculados  { get; init; }   // activos hoy
    public int TotalEgresados     { get; init; }
    public int TotalDesertores    { get; init; }
    public int TotalHistorico     { get; init; }   // todos los ingresados

    // Riesgo académico (sobre activos)
    public int     RiesgoAlto              { get; init; }
    public int     RiesgoMedio             { get; init; }
    public int     RiesgoBajo              { get; init; }
    public decimal PorcentajeRiesgoAlto    { get; init; }

    // Tasas institucionales
    public decimal TasaRetencionGlobal  { get; init; }
    public decimal TasaDesercionGlobal  { get; init; }
    public decimal TasaEgresoGlobal     { get; init; }

    // Rendimiento
    public decimal? PromedioNotaGlobal          { get; init; }
    public decimal  PorcentajeAprobacionGlobal  { get; init; }

    // Evolución por cohorte para el gráfico de barras
    public List<EvolucionCohorteResumenDto> EvolucionCohortes { get; init; } = [];

    public DateTime GeneradoEn { get; init; }
}

public class EvolucionCohorteResumenDto
{
    public int AnioCohorte { get; init; }
    public int Total       { get; init; }
    public int Activos     { get; init; }
    public int Egresados   { get; init; }
    public int Desertores  { get; init; }
}
