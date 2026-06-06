using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Reportes.DTOs;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Tests.Reportes;

/// <summary>
/// Fake configurable de IRendimientoConsolidadoRepository para tests de UseCases.
/// Cada propiedad configura lo que devuelve el método correspondiente.
/// </summary>
internal class RendimientoRepoFake : IRendimientoConsolidadoRepository
{
    public List<DatosRiesgoEstudianteDto> DatosRiesgo { get; set; } = [];
    public List<DatosCohorteDto>          DatosCohorte { get; set; } = [];
    public List<FilaPromedioCatedraDto>   DatosCatedras { get; set; } = [];
    public List<PuntoMatriculaDto>        EvolucionMatricula { get; set; } = [];
    public List<(string Carrera, int AnioCohorte, int TotalEgresados, int TotalAlumnos, double? DuracionPromedioAnios)> DatosEgresados { get; set; } = [];

    public Task<List<DatosRiesgoEstudianteDto>> ObtenerDatosRiesgoAsync(
        int? anioCohorte, int? carreraId, CancellationToken ct = default)
        => Task.FromResult(DatosRiesgo);

    public Task<List<DatosCohorteDto>> ObtenerDatosCohorteAsync(
        int? carreraId, int? anioCohorte, CancellationToken ct = default)
        => Task.FromResult(DatosCohorte);

    public Task<List<(string, int, int, int, double?)>> ObtenerEgresadosPorCarreraAsync(
        int? carreraId, int? anioCohorte, CancellationToken ct = default)
        => Task.FromResult(DatosEgresados);

    public Task<IEnumerable<FilaPromedioCatedraDto>> ObtenerPromediosCatedraAsync(
        int? docenteId, int? anio, int? cursoId, int? carreraId, CancellationToken ct = default)
        => Task.FromResult<IEnumerable<FilaPromedioCatedraDto>>(DatosCatedras);

    public Task<List<PuntoMatriculaDto>> ObtenerEvolucionMatriculaAsync(CancellationToken ct = default)
        => Task.FromResult(EvolucionMatricula);

    // Métodos no usados en estos tests — devuelven valores vacíos.
    public Task<List<int>> ObtenerAniosCohorteAsync(int? carreraId, CancellationToken ct = default)
        => Task.FromResult(new List<int>());

    public Task<List<DatosRetencionAnualRawDto>> ObtenerDatosRetencionAnualAsync(
        int? carreraId, int? anioCohorte, CancellationToken ct = default)
        => Task.FromResult(new List<DatosRetencionAnualRawDto>());

    public Task<(List<(int, int, int)>, int, int)> ObtenerDesercionPorAnioAsync(
        int? carreraId, int? anioCohorte, CancellationToken ct = default)
        => Task.FromResult((new List<(int, int, int)>(), 0, 0));

    public Task<IEnumerable<FilaComparativoComisionDto>> ObtenerComparativoComisionesAsync(
        int? materiaId, int? anio, int? docenteId, CancellationToken ct = default)
        => Task.FromResult<IEnumerable<FilaComparativoComisionDto>>([]);

    public Task<IEnumerable<PuntoEvolucionNotaDto>> ObtenerEvolucionNotasAsync(
        int? materiaId, int? anio, int? docenteId,
        int? cuatrimestre, byte? anioCarrera, TipoExamen? tipoExamen,
        string granularidad = "mensual", CancellationToken ct = default)
        => Task.FromResult<IEnumerable<PuntoEvolucionNotaDto>>([]);

    public Task<string?> ObtenerNombreMateriaAsync(int materiaId, CancellationToken ct = default)
        => Task.FromResult<string?>(null);
}
