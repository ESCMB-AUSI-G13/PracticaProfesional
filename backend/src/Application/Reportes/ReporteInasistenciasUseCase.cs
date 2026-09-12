using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes.DTOs;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Reportes;

/// <summary>
/// RR-08: Reporte de inasistencias detalladas (Nivel Operativo).
///
/// Genera un listado de registros de asistencia (por defecto solo ausencias) con
/// información del estudiante, materia, curso, fecha y motivo de justificación.
/// Permite filtrar por curso, materia y rango de fechas.
/// </summary>
public class ReporteInasistenciasUseCase(IAsistenciaRepository asistenciaRepository)
{
    /// <param name="incluirTodosLosRegistros">
    /// true para exportar a PDF: ignora la paginación del filtro y trae todos los registros que
    /// matchean (el PDF tiene que reflejar el filtro completo, no una sola página).
    /// </param>
    public async Task<ReporteInasistenciasDto> EjecutarAsync(
        FiltroInasistenciasDto filtro,
        IReadOnlyList<(int MateriaId, int CursoId)>? espaciosPermitidos = null,
        bool incluirTodosLosRegistros = false,
        CancellationToken cancellationToken = default)
    {
        int? pagina       = incluirTodosLosRegistros ? null : filtro.Pagina;
        int? tamanoPagina = incluirTodosLosRegistros ? null : filtro.TamanoPagina;

        var (registros, totalRegistros, totalAusentes, totalAusentesJust, totalPresentes, porMateriaComision) =
            await asistenciaRepository.ObtenerConDetalleAsync(
                filtro.CursoId,
                filtro.MateriaId,
                filtro.FechaDesde,
                filtro.FechaHasta,
                filtro.SoloAusencias,
                filtro.Comision,
                filtro.AnioLectivo,
                espaciosPermitidos,
                pagina,
                tamanoPagina,
                cancellationToken);

        var items = registros.Select(a => new RegistroInasistenciaDto
        {
            EstudianteId  = a.EstudianteId,
            Legajo        = a.Estudiante.Usuario.Legajo,
            NombreCompleto = $"{a.Estudiante.Usuario.Apellido}, {a.Estudiante.Usuario.Nombre}",
            Materia       = a.Materia.Nombre,
            Curso         = $"{a.Curso.AnioLectivo}° {a.Curso.Comision}",
            Fecha         = a.Fecha,
            TipoAsistencia = a.Estado.ToString(),
            Motivo        = a.Motivo
        }).ToList();

        return new ReporteInasistenciasDto
        {
            GeneradoEn                = DateTime.UtcNow,
            TotalRegistros            = totalRegistros,
            TotalAusentes             = totalAusentes,
            TotalAusentesJustificados = totalAusentesJust,
            TotalPresentes            = totalPresentes,
            Pagina                    = pagina ?? 1,
            TamanoPagina              = tamanoPagina ?? totalRegistros,
            TotalPaginas              = tamanoPagina.HasValue && tamanoPagina.Value > 0
                ? (int)Math.Ceiling(totalRegistros / (double)tamanoPagina.Value)
                : (totalRegistros > 0 ? 1 : 0),
            Registros                 = items,
            PorMateriaComision        = porMateriaComision
        };
    }
}
