using PracticaProfesional.Application.Asistencias.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Asistencias;

public class ObtenerAlumnosPorEspacioUseCase(
    IEspacioCurricularRepository espacioRepository,
    IInscripcionMateriaRepository inscripcionRepository)
{
    public async Task<IEnumerable<AlumnoParaAsistenciaDto>> EjecutarAsync(
        int espacioCurricularId,
        CancellationToken cancellationToken = default)
    {
        var espacio = await espacioRepository.ObtenerPorIdAsync(espacioCurricularId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el espacio curricular con Id {espacioCurricularId}.");

        var inscripciones = await inscripcionRepository.ListarActivasPorCursoYMateriaAsync(
            espacio.CursoId, espacio.MateriaId, cancellationToken);

        return inscripciones.Select(i => new AlumnoParaAsistenciaDto(
            EstudianteId: i.EstudianteId,
            NombreCompleto: $"{i.Estudiante.Usuario.Apellido}, {i.Estudiante.Usuario.Nombre}",
            Legajo: i.Estudiante.Usuario.Legajo));
    }
}
