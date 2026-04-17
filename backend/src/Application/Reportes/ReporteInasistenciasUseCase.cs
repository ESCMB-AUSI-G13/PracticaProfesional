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
    public async Task<ReporteInasistenciasDto> EjecutarAsync(
        FiltroInasistenciasDto filtro,
        CancellationToken cancellationToken = default)
    {
        var registros = await asistenciaRepository.ObtenerConDetalleAsync(
            filtro.CursoId,
            filtro.MateriaId,
            filtro.FechaDesde,
            filtro.FechaHasta,
            filtro.SoloAusencias,
            cancellationToken);

        var items = registros.Select(a => new RegistroInasistenciaDto
        {
            EstudianteId  = a.EstudianteId,
            Legajo        = a.Estudiante.Usuario.Legajo,
            NombreCompleto = $"{a.Estudiante.Usuario.Apellido}, {a.Estudiante.Usuario.Nombre}",
            Materia       = a.Materia.Nombre,
            Curso         = $"{a.Curso.Anio} – {a.Curso.Comision}",
            Fecha         = a.Fecha,
            TipoAsistencia = a.Estado.ToString(),
            Motivo        = a.Motivo
        }).ToList();

        return new ReporteInasistenciasDto
        {
            GeneradoEn                = DateTime.UtcNow,
            TotalRegistros            = items.Count,
            TotalAusentes             = items.Count(r => r.TipoAsistencia == EstadoAsistencia.Ausente.ToString()),
            TotalAusentesJustificados = items.Count(r => r.TipoAsistencia == EstadoAsistencia.AusenteJustificado.ToString()),
            Registros                 = items
        };
    }
}
