namespace PracticaProfesional.Application.Reportes.DTOs;

/// <summary>
/// Cantidad de registros que matchean el filtro del reporte de inasistencias (RR-08), agrupados
/// por materia y comisión — alimenta el gráfico comparativo del frontend. Se calcula sobre TODO
/// lo que matchea el filtro, no solo la página visible de <see cref="ReporteInasistenciasDto.Registros"/>.
/// </summary>
public class ConteoMateriaComisionDto
{
    public string Materia { get; set; } = string.Empty;
    public string Comision { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}
