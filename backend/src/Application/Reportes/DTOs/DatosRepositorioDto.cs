namespace PracticaProfesional.Application.Reportes.DTOs;

// Datos crudos por estudiante — devueltos por el repositorio, procesados por los UseCases.
public class DatosRiesgoEstudianteDto
{
    public int     EstudianteId   { get; init; }
    public string  Legajo         { get; init; } = string.Empty;
    public string  Nombre         { get; init; } = string.Empty;
    public string  Apellido       { get; init; } = string.Empty;
    public string  Carrera        { get; init; } = string.Empty;
    public int     AnioCarrera    { get; init; }
    public int     AnioCohorte    { get; init; }
    public string  Condicion      { get; init; } = string.Empty;
    public int     TotalClases    { get; init; }
    public int     Ausencias      { get; init; }
    public decimal? PromedioNotas { get; init; }
    public int     Reprobadas     { get; init; }
    public int     TotalExamenes  { get; init; }
    public DateTime? UltimaAsistencia { get; init; }
}

// Datos crudos por cohorte — devueltos por el repositorio para el reporte de retención.
public class DatosCohorteDto
{
    public int    AnioCohorte  { get; init; }
    public string Carrera      { get; init; } = string.Empty;
    public int    Total        { get; init; }
    public int    Activos      { get; init; }
    public int    Egresados    { get; init; }
    public int    Desertores   { get; init; }
}

// Datos crudos para RR-12: un registro por (estudiante × año de historial).
// Un mismo estudiante puede aparecer N veces (una por cada año que tiene en HistorialAcademico).
// Si no tiene ningún registro, aparece una sola vez con AnioHistorial = -1.
public class DatosRetencionAnualRawDto
{
    public int    AnioCohorte   { get; init; }
    public string Carrera       { get; init; } = string.Empty;
    public int    EstudianteId  { get; init; }
    public int    AnioHistorial { get; init; }  // -1 cuando no hay registros en HistorialAcademico
    public bool   EsDesertor    { get; init; }
}
