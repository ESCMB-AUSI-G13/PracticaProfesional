using PracticaProfesional.Application.Reportes.DTOs;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

/// <summary>
/// Repositorio de asistencias. Proporciona consultas sobre presencia/ausencia de estudiantes.
/// </summary>
public interface IAsistenciaRepository
{
    /// <summary>
    /// Devuelve estadísticas de asistencia de un estudiante para una materia y curso dados.
    /// </summary>
    Task<(int Total, int AusentesInjustificados, int Presentes)> ObtenerEstadisticasAsync(
        int estudianteId,
        int materiaId,
        int cursoId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve la fecha de la última asistencia registrada para el estudiante.
    /// Retorna <c>null</c> si no hay registros.
    /// </summary>
    Task<DateTime?> ObtenerUltimaFechaActividadAsync(
        int estudianteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// RR-08: Devuelve registros de asistencia con navegación cargada (Estudiante→Usuario,
    /// Materia, Curso), aplicando filtros opcionales de curso, materia, rango de fechas
    /// y si se incluyen sólo ausencias. Los totales devueltos son sobre TODO lo que matchea el
    /// filtro, no solo la página; <paramref name="pagina"/>/<paramref name="tamanoPagina"/> en
    /// null (usado para exportar a PDF) devuelve todos los registros sin recortar.
    /// </summary>
    Task<(IReadOnlyList<Asistencia> Registros, int TotalRegistros, int TotalAusentes, int TotalAusentesJustificados, int TotalPresentes, IReadOnlyList<ConteoMateriaComisionDto> PorMateriaComision)> ObtenerConDetalleAsync(
        int? cursoId,
        int? materiaId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        bool soloAusencias,
        string? comision = null,
        int? anioLectivo = null,
        IReadOnlyList<(int MateriaId, int CursoId)>? espaciosPermitidos = null,
        int? pagina = null,
        int? tamanoPagina = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// RR-09: Devuelve todos los registros de asistencia de un estudiante con navegación
    /// cargada, ordenados por materia y fecha.
    /// </summary>
    Task<IEnumerable<Asistencia>> ObtenerPorEstudianteAsync(
        int estudianteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si ya existen registros de asistencia para un curso/materia en una fecha dada.
    /// </summary>
    Task<bool> ExistePorCursoMateriaFechaAsync(int cursoId, int materiaId, DateTime fecha, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserta múltiples registros de asistencia en un único guardado.
    /// </summary>
    Task RegistrarBulkAsync(IEnumerable<Asistencia> asistencias, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve todos los registros de asistencia de un curso/materia en una fecha dada,
    /// con navegación de Estudiante→Usuario cargada.
    /// </summary>
    Task<IEnumerable<Asistencia>> ObtenerPorEspacioYFechaAsync(
        int cursoId, int materiaId, DateTime fecha, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste cambios pendientes en el contexto (rectificaciones).
    /// </summary>
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
