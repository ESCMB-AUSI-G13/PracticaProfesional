using PracticaProfesional.Application.Reportes.DTOs;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Interfaces;

public interface IRendimientoConsolidadoRepository
{
    Task<List<DatosRiesgoEstudianteDto>> ObtenerDatosRiesgoAsync(
        int? anioCohorte, int? carreraId, CancellationToken ct = default);

    Task<List<DatosCohorteDto>> ObtenerDatosCohorteAsync(
        int? carreraId, int? anioCohorte, CancellationToken ct = default);

    Task<List<int>> ObtenerAniosCohorteAsync(int? carreraId, CancellationToken ct = default);

    Task<List<DatosRetencionAnualRawDto>> ObtenerDatosRetencionAnualAsync(
        int? carreraId, int? anioCohorte, CancellationToken ct = default);

    Task<(List<(int AnioCursada, int Total, int Desertores)> Filas, int TotalGlobal, int DesertoresGlobal)> ObtenerDesercionPorAnioAsync(
        int? carreraId, int? anioCohorte, CancellationToken ct = default);

    Task<List<(string Carrera, int AnioCohorte, int TotalEgresados, int TotalAlumnos, double? DuracionPromedioAnios)>> ObtenerEgresadosPorCarreraAsync(
        int? carreraId, int? anioCohorte, CancellationToken ct = default);

    Task<List<PuntoMatriculaDto>> ObtenerEvolucionMatriculaAsync(CancellationToken ct = default);


    Task<IEnumerable<FilaComparativoComisionDto>> ObtenerComparativoComisionesAsync(
        int? materiaId, int? anio, int? docenteId, CancellationToken ct = default);

    Task<IEnumerable<PuntoEvolucionNotaDto>> ObtenerEvolucionNotasAsync(
        int? materiaId, int? anio, int? docenteId,
        int? cuatrimestre, byte? anioCarrera, TipoExamen? tipoExamen,
        string granularidad = "mensual",
        CancellationToken ct = default);

    Task<IEnumerable<FilaPromedioCatedraDto>> ObtenerPromediosCatedraAsync(
        int? docenteId, int? anio, int? cursoId, int? carreraId, CancellationToken ct = default);

    Task<string?> ObtenerNombreMateriaAsync(int materiaId, CancellationToken ct = default);
}
