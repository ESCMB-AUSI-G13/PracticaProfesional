using PracticaProfesional.Application.Calificaciones.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Calificaciones;

/// <summary>
/// Retorna el listado de inscripciones de un examen para que el docente
/// pueda ver qué alumnos están anotados y cuáles ya tienen nota cargada.
/// </summary>
public class ListarInscripcionesExamenUseCase(
    IInscripcionExamenRepository inscripcionExamenRepository,
    IExamenRepository examenRepository,
    IDocenteRepository docenteRepository,
    IEspacioCurricularRepository espacioCurricularRepository)
{
    public async Task<IEnumerable<InscripcionExamenDto>> EjecutarAsync(
        int examenId,
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var examen = await examenRepository.ObtenerPorIdAsync(examenId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el examen con Id {examenId}.");

        // Solo el docente a cargo de la materia puede ver el acta de su examen
        if (!await EsDocenteDeLaMateriaAsync(usuarioId, examen.MateriaId, cancellationToken))
            throw new BusinessException("No tenés permiso para ver las inscripciones de este examen.", 403);

        var inscripciones = await inscripcionExamenRepository
            .ObtenerPorExamenAsync(examenId, cancellationToken);

        return inscripciones.Select(i => new InscripcionExamenDto(
            Id: i.Id,
            EstudianteId: i.EstudianteId,
            EstudianteNombreCompleto: $"{i.Estudiante.Usuario.Nombre} {i.Estudiante.Usuario.Apellido}",
            EstudianteLegajo: i.Estudiante.Usuario.Legajo,
            ExamenId: i.ExamenId,
            TipoExamen: i.Examen.TipoExamen.ToString(),
            MateriaNombre: i.Examen.Materia.Nombre,
            NotaValor: i.NotaValor,
            EsAprobado: i.NotaValor.HasValue ? i.NotaValor >= 4 : null,
            Estado: i.Estado.ToString(),
            FechaInscripcion: i.FechaInscripcion));
    }

    /// <summary>Mismo criterio que ListarEncuestasDocenteUseCase.EsMateriaDelDocenteAsync.</summary>
    private async Task<bool> EsDocenteDeLaMateriaAsync(int usuarioId, int materiaId, CancellationToken cancellationToken)
    {
        var docente = await docenteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
        if (docente is null) return false;

        var espacios = await espacioCurricularRepository.ListarPorDocenteIdAsync(docente.Id, cancellationToken);
        return espacios.Any(e => e.MateriaId == materiaId);
    }
}
