using PracticaProfesional.Application.Inscripciones.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Inscripciones;

public class ListarMisInscripcionesEstudianteUseCase(
    IEstudianteRepository estudianteRepository,
    IInscripcionMateriaRepository inscripcionMateriaRepository)
{
    public async Task<IEnumerable<InscripcionMateriaListadoDto>> EjecutarAsync(
        int usuarioId, CancellationToken cancellationToken = default)
    {
        var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el perfil de estudiante para el usuario {usuarioId}.");

        var inscripciones = await inscripcionMateriaRepository.ListarActivasPorEstudianteAsync(
            estudiante.Id, cancellationToken);

        return inscripciones.Select(i => new InscripcionMateriaListadoDto(
            i.Id,
            i.EstudianteId,
            $"{i.Estudiante.Usuario.Apellido}, {i.Estudiante.Usuario.Nombre}",
            i.MateriaId,
            i.Materia?.Codigo ?? string.Empty,
            i.Materia?.Nombre ?? string.Empty,
            i.CursoId,
            i.Curso?.AnioLectivo ?? 0,
            i.Curso?.Comision ?? string.Empty,
            i.Estado.ToString(),
            i.FechaInscripcion
        ));
    }
}
