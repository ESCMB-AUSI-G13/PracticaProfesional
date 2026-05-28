using PracticaProfesional.Application.Examenes.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Examenes;

public class CrearExamenUseCase(
    IExamenRepository examenRepository,
    IMateriaRepository materiaRepository,
    IInscripcionMateriaRepository inscripcionMateriaRepository,
    IInscripcionExamenRepository inscripcionExamenRepository,
    IAuditoriaService auditoria)
{
    private static readonly HashSet<TipoExamen> _tiposAutoInscripcion =
    [
        TipoExamen.Parcial,
        TipoExamen.Recuperatorio
    ];

    public async Task<ExamenDto> EjecutarAsync(CrearExamenDto dto, CancellationToken cancellationToken = default)
    {
        var materia = await materiaRepository.ObtenerPorIdAsync(dto.MateriaId, cancellationToken)
            ?? throw new BusinessException($"No se encontró la materia con Id {dto.MateriaId}.");

        if (!Enum.TryParse<TipoExamen>(dto.TipoExamen, ignoreCase: true, out var tipo))
            throw new BusinessException($"Tipo de examen '{dto.TipoExamen}' no válido.");

        var examen = Examen.Crear(materia.Id, dto.FechaExamen, dto.Horario, dto.Cupo, tipo);
        await examenRepository.AgregarAsync(examen, cancellationToken);

        // Auto-inscripción para parciales y recuperatorios
        if (_tiposAutoInscripcion.Contains(tipo))
        {
            var activos = await inscripcionMateriaRepository
                .ListarActivasPorMateriaAsync(materia.Id, cancellationToken);

            var inscripciones = activos
                .Select(i => InscripcionExamen.Crear(i.EstudianteId, examen.Id))
                .ToList();

            if (inscripciones.Count > 0)
                await inscripcionExamenRepository.AgregarRangoAsync(inscripciones, cancellationToken);
        }

        await auditoria.RegistrarAsync("Examen", examen.Id.ToString(), "CREAR",
            valorAnterior: null,
            valorNuevo: new { examen.MateriaId, examen.FechaExamen, examen.TipoExamen, examen.Cupo },
            cancellationToken: cancellationToken);

        return ToDto(examen, materia.Nombre, materia.Codigo);
    }

    internal static ExamenDto ToDto(Examen e, string materiaNombre, string materiaCodigo) => new(
        e.Id, e.MateriaId, materiaNombre, materiaCodigo, e.FechaExamen, e.Horario, e.Cupo, e.TipoExamen.ToString());
}
