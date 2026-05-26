namespace PracticaProfesional.Application.Reportes.DTOs;

public class RiesgoAcademicoDto
{
    public int    EstudianteId            { get; init; }
    public string Legajo                  { get; init; } = string.Empty;
    public string NombreCompleto          { get; init; } = string.Empty;
    public string Carrera                 { get; init; } = string.Empty;
    public int    AnioCarrera             { get; init; }
    public int    AnioCohorte             { get; init; }
    public string Condicion               { get; init; } = string.Empty;
    public string NivelRiesgo             { get; init; } = string.Empty;  // Bajo | Medio | Alto
    public decimal PorcentajeInasistencias { get; init; }
    public decimal? PromedioNotas         { get; init; }
    public int    MateriasReprobadas      { get; init; }
}

public class ReporteRiesgoAcademicoDto
{
    public List<RiesgoAcademicoDto> Estudiantes { get; init; } = [];
    public int TotalAlto                        { get; init; }
    public int TotalMedio                       { get; init; }
    public int TotalBajo                        { get; init; }
}
