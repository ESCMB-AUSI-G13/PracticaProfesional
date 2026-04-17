namespace PracticaProfesional.Application.Reportes.DTOs;

/// <summary>
/// Control individual de asistencia por legajo (RR-09).
/// Agrupa el perfil del estudiante y el resumen de asistencia por cada materia cursada.
/// </summary>
public class ControlLegajoDto
{
    public int EstudianteId { get; set; }
    public string Legajo { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string CondicionAcademica { get; set; } = string.Empty;
    public int Anio { get; set; }
    public DateTime FechaDeIngreso { get; set; }

    /// <summary>Detalle de asistencia agrupado por materia y curso.</summary>
    public IEnumerable<ResumenAsistenciaMateriaDto> AsistenciasPorMateria { get; set; } =
        Enumerable.Empty<ResumenAsistenciaMateriaDto>();

    // ── Totales globales ────────────────────────────────────────────────────────
    public int TotalClasesGlobal { get; set; }
    public int TotalPresentesGlobal { get; set; }
    public int TotalAusentesJustificadosGlobal { get; set; }
    public int TotalAusentesInjustificadosGlobal { get; set; }

    /// <summary>Porcentaje global de presencia efectiva sobre todas las materias.</summary>
    public decimal PorcentajePresenciaGlobal { get; set; }

    /// <summary>Cantidad de materias en zona de riesgo (ausencias > 20 %).</summary>
    public int MateriasEnRiesgo { get; set; }

    /// <summary>Cantidad de materias con regularidad ya perdida (ausencias > 25 %).</summary>
    public int MateriasConRegularidadPerdida { get; set; }

    /// <summary>Fecha y hora en que se generó el reporte.</summary>
    public DateTime GeneradoEn { get; set; }
}
