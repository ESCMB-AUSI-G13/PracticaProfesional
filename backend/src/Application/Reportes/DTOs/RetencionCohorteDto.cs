namespace PracticaProfesional.Application.Reportes.DTOs;

public class RetencionCohorteDto
{
    public int     AnioCohorte     { get; init; }
    public string  Carrera         { get; init; } = string.Empty;
    public int     Total           { get; init; }
    public int     Activos         { get; init; }
    public int     Egresados       { get; init; }
    public int     Desertores      { get; init; }
    public decimal TasaRetencion   { get; init; }  // (Activos + Egresados) / Total %
    public decimal TasaDesercion   { get; init; }  // Desertores / Total %
    public decimal TasaEgreso      { get; init; }  // Egresados / Total %
}

public class ReporteRetencionCohorteDto
{
    public List<RetencionCohorteDto> Cohortes         { get; init; } = [];
    public int     TotalGeneral                       { get; init; }
    public decimal TasaRetencionGlobal                { get; init; }
    public decimal TasaDesercionGlobal                { get; init; }
}
