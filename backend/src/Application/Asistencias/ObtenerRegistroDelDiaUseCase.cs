using PracticaProfesional.Application.Asistencias.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Asistencias;

public class ObtenerRegistroDelDiaUseCase(
    IEspacioCurricularRepository espacioRepository,
    IAsistenciaRepository asistenciaRepository)
{
    public async Task<RegistroDelDiaDto> EjecutarAsync(
        int espacioCurricularId,
        DateTime fecha,
        CancellationToken cancellationToken = default)
    {
        var espacio = await espacioRepository.ObtenerPorIdAsync(espacioCurricularId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el espacio curricular con Id {espacioCurricularId}.", 404);

        var asistencias = await asistenciaRepository.ObtenerPorEspacioYFechaAsync(
            espacio.CursoId, espacio.MateriaId, fecha.Date, cancellationToken);

        if (!asistencias.Any())
            throw new BusinessException(
                $"No existe registro de asistencia para la fecha {fecha:dd/MM/yyyy}.", 404);

        var alumnos = asistencias.Select(a => new AsistenciaDetalleDto(
            AsistenciaId: a.Id,
            EstudianteId: a.EstudianteId,
            NombreCompleto: $"{a.Estudiante.Usuario.Apellido}, {a.Estudiante.Usuario.Nombre}",
            Legajo: a.Estudiante.Usuario.Legajo,
            Estado: a.Estado.ToString(),
            Motivo: a.Motivo));

        return new RegistroDelDiaDto(
            EspacioCurricularId: espacio.Id,
            CursoId: espacio.CursoId,
            MateriaId: espacio.MateriaId,
            MateriaNombre: espacio.Materia.Nombre,
            AnioLectivo: espacio.Curso.AnioLectivo,
            Comision: espacio.Curso.Comision,
            Fecha: fecha.Date,
            Alumnos: alumnos);
    }
}
