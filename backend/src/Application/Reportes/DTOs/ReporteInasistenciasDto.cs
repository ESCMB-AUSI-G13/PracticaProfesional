namespace PracticaProfesional.Application.Reportes.DTOs;

/// <summary>
/// Respuesta del reporte detallado de inasistencias (RR-08).
/// Incluye un resumen agregado y la lista de registros individuales.
/// </summary>
public class ReporteInasistenciasDto
{
    /// <summary>Fecha y hora en que se generó el reporte.</summary>
    public DateTime GeneradoEn { get; set; }

    public int TotalRegistros { get; set; }
    public int TotalAusentes { get; set; }
    public int TotalAusentesJustificados { get; set; }

    public IEnumerable<RegistroInasistenciaDto> Registros { get; set; } =
        Enumerable.Empty<RegistroInasistenciaDto>();
}
