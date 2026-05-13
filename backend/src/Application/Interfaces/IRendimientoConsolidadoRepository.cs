using PracticaProfesional.Application.Reportes.DTOs;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Interfaces;

public interface IRendimientoConsolidadoRepository
{
    Task<IEnumerable<FilaComparativoComisionDto>> ObtenerComparativoComisionesAsync(
        int? materiaId, int? anio, int? docenteId, CancellationToken ct = default);

    Task<IEnumerable<PuntoEvolucionNotaDto>> ObtenerEvolucionNotasAsync(
        int? materiaId, int? anio, int? docenteId,
        int? cuatrimestre, byte? anioCarrera, TipoExamen? tipoExamen,
        CancellationToken ct = default);

    Task<IEnumerable<FilaPromedioCatedraDto>> ObtenerPromediosCatedraAsync(
        int? docenteId, int? anio, int? cursoId, CancellationToken ct = default);

    Task<string?> ObtenerNombreMateriaAsync(int materiaId, CancellationToken ct = default);
}
