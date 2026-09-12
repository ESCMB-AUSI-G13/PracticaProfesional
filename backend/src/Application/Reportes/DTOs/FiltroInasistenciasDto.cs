namespace PracticaProfesional.Application.Reportes.DTOs;

/// <summary>
/// Filtros de entrada para el reporte de inasistencias (RR-08).
/// Los filtros de contenido son opcionales; sin ninguno se evalúan todos los registros, pero el
/// listado que viaja al cliente siempre viene paginado por <see cref="Pagina"/>/<see cref="TamanoPagina"/>
/// (antes no había límite y una consulta sin filtros devolvía cientos de miles de filas en una sola respuesta).
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

    /// <summary>Página solicitada (1-based).</summary>
    public int Pagina { get; set; } = 1;

    /// <summary>Cantidad de registros por página. El frontend ofrece 10/20/50.</summary>
    public int TamanoPagina { get; set; } = 20;
}
