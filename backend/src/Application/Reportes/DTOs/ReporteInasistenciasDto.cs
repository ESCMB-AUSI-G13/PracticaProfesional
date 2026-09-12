namespace PracticaProfesional.Application.Reportes.DTOs;

/// <summary>
/// Respuesta del reporte detallado de inasistencias (RR-08).
/// Incluye un resumen agregado y la lista de registros individuales.
/// </summary>
public class ReporteInasistenciasDto
{
    /// <summary>Fecha y hora en que se generó el reporte.</summary>
    public DateTime GeneradoEn { get; set; }

    /// <summary>Total de registros que matchean los filtros (no solo los de esta página).</summary>
    public int TotalRegistros { get; set; }
    public int TotalAusentes { get; set; }
    public int TotalAusentesJustificados { get; set; }
    public int TotalPresentes { get; set; }

    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalPaginas { get; set; }

    /// <summary>Solo los registros de la página solicitada.</summary>
    public IEnumerable<RegistroInasistenciaDto> Registros { get; set; } =
        Enumerable.Empty<RegistroInasistenciaDto>();

    /// <summary>Conteo por materia/comisión sobre TODO el filtro, para el gráfico comparativo (no se recorta por página).</summary>
    public IEnumerable<ConteoMateriaComisionDto> PorMateriaComision { get; set; } =
        Enumerable.Empty<ConteoMateriaComisionDto>();
}
