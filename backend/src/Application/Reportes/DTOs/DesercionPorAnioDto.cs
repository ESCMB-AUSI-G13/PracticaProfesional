namespace PracticaProfesional.Application.Reportes.DTOs;

public class DesercionPorAnioDto
{
    public int     AnioCursada      { get; init; }
    public int     TotalEstudiantes { get; init; }
    public int     Desertores       { get; init; }
    public decimal TasaDesercion    { get; init; }
    public string  NivelRiesgo      { get; init; } = string.Empty;
}

public class ReporteDesercionPorAnioDto
{
    public List<DesercionPorAnioDto> Filas            { get; init; } = [];
    public int                       TotalEstudiantes { get; init; }
    public int                       TotalDesertores  { get; init; }
    public decimal                   TasaGlobal       { get; init; }
}
