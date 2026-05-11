using PracticaProfesional.Application.Asistencias.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Asistencias;

public class ObtenerEspaciosPorDocenteUseCase(
    IDocenteRepository docenteRepository,
    IEspacioCurricularRepository espacioRepository)
{
    public async Task<IEnumerable<EspacioAsistenciaDto>> EjecutarAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var docente = await docenteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new BusinessException("No se encontró un perfil de docente para el usuario autenticado.");

        var espacios = await espacioRepository.ListarPorDocenteIdAsync(docente.Id, cancellationToken);

        return espacios.Select(ec => new EspacioAsistenciaDto(
            EspacioCurricularId: ec.Id,
            CursoId: ec.CursoId,
            MateriaId: ec.MateriaId,
            MateriaNombre: ec.Materia.Nombre,
            AnioLectivo: ec.Curso.AnioLectivo,
            Comision: ec.Curso.Comision));
    }
}
