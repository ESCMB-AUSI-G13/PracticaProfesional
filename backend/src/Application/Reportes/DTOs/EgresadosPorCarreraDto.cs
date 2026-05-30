namespace PracticaProfesional.Application.Reportes.DTOs;

public class FilaEgresadoCarreraDto
{
    public string Carrera             { get; init; } = string.Empty;
    public int    AnioCohorte         { get; init; }
    public int    TotalEgresados      { get; init; }
    public int    TotalAlumnosCohorte { get; init; }
    public double TasaEgreso          { get; init; }
    public double? DuracionPromedioAnios { get; init; }
}

public class ResumenCarreraEgresadosDto
{
    public string Carrera { get; init; } = string.Empty;
    public int    Total   { get; init; }
}

public class ReporteEgresadosPorCarreraDto
{
    public List<FilaEgresadoCarreraDto>     Filas                  { get; init; } = [];
    public List<ResumenCarreraEgresadosDto> PorCarrera             { get; init; } = [];
    public int                              TotalGeneral           { get; init; }
    public double                           TasaEgresoGlobal       { get; init; }
    public double?                          DuracionPromedioGlobal { get; init; }
}
