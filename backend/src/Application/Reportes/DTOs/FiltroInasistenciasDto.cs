namespace PracticaProfesional.Application.Reportes.DTOs;

/// <summary>
/// Filtros de entrada para el reporte de inasistencias (RR-08).
/// Todos los parámetros son opcionales; sin filtros se devuelven todos los registros.
/// </summary>
public class FiltroInasistenciasDto
{
    /// <summary>Filtra por curso específico.</summary>
    public int? CursoId { get; set; }

    /// <summary>Filtra por año lectivo del plan (1, 2, 3, 4).</summary>
    public int? AnioLectivo { get; set; }

    /// <summary>Filtra por materia específica.</summary>
    public int? MateriaId { get; set; }

    /// <summary>Fecha de inicio del rango (inclusive). Se ignora la hora.</summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>Fecha de fin del rango (inclusive). Se ignora la hora.</summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>Filtra por comisión (ej. "A" o "B"). Insensible a mayúsculas.</summary>
    public string? Comision { get; set; }

    /// <summary>
    /// Si es <c>true</c> (valor por defecto), devuelve sólo ausencias (justificadas e injustificadas).
    /// Si es <c>false</c>, incluye también las presencias.
    /// </summary>
    public bool SoloAusencias { get; set; } = true;
}
