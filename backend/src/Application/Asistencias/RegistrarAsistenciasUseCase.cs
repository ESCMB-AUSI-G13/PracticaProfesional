using PracticaProfesional.Application.Asistencias.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Asistencias;

public class RegistrarAsistenciasUseCase(
    IEspacioCurricularRepository espacioRepository,
    IInscripcionMateriaRepository inscripcionRepository,
    IAsistenciaRepository asistenciaRepository)
{
    public async Task<ResumenAsistenciasDto> EjecutarAsync(
        RegistrarAsistenciasCommand command,
        CancellationToken cancellationToken = default)
    {
        var espacio = await espacioRepository.ObtenerPorIdAsync(command.EspacioCurricularId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el espacio curricular con Id {command.EspacioCurricularId}.");

        var fecha = command.Fecha.Date;

        var yaRegistrado = await asistenciaRepository.ExistePorCursoMateriaFechaAsync(
            espacio.CursoId, espacio.MateriaId, fecha, cancellationToken);

        if (yaRegistrado)
            throw new BusinessException(
                $"Ya existe un registro de asistencia para esta materia y curso en la fecha {fecha:dd/MM/yyyy}.");

        var inscripciones = await inscripcionRepository.ListarActivasPorCursoYMateriaAsync(
            espacio.CursoId, espacio.MateriaId, cancellationToken);

        var ausentesMap = command.Ausentes.ToDictionary(a => a.EstudianteId);

        var registros = new List<Asistencia>();
        foreach (var inscripcion in inscripciones)
        {
            if (ausentesMap.TryGetValue(inscripcion.EstudianteId, out var ausencia))
            {
                var estado = ausencia.TipoAusencia == "Justificada"
                    ? EstadoAsistencia.AusenteJustificado
                    : EstadoAsistencia.Ausente;

                registros.Add(Asistencia.Registrar(
                    inscripcion.EstudianteId,
                    espacio.MateriaId,
                    espacio.CursoId,
                    fecha,
                    estado,
                    ausencia.Motivo));
            }
            else
            {
                registros.Add(Asistencia.Registrar(
                    inscripcion.EstudianteId,
                    espacio.MateriaId,
                    espacio.CursoId,
                    fecha,
                    EstadoAsistencia.Presente));
            }
        }

        await asistenciaRepository.RegistrarBulkAsync(registros, cancellationToken);

        var ausentes = registros.Where(r => r.Estado != EstadoAsistencia.Presente).ToList();
        var detalleAusentes = ausentes.Select(r =>
        {
            var insc = inscripciones.First(i => i.EstudianteId == r.EstudianteId);
            return new DetalleAusenciaDto(
                NombreCompleto: $"{insc.Estudiante.Usuario.Apellido}, {insc.Estudiante.Usuario.Nombre}",
                Legajo: insc.Estudiante.Usuario.Legajo,
                TipoAusencia: r.Estado == EstadoAsistencia.AusenteJustificado ? "Justificada" : "Injustificada",
                Motivo: r.Motivo);
        });

        return new ResumenAsistenciasDto(
            Fecha: fecha,
            MateriaNombre: espacio.Materia.Nombre,
            CursoComision: espacio.Curso.Comision,
            AnioLectivo: espacio.Curso.AnioLectivo,
            TotalAlumnos: registros.Count,
            Presentes: registros.Count(r => r.Estado == EstadoAsistencia.Presente),
            AusentesInjustificados: registros.Count(r => r.Estado == EstadoAsistencia.Ausente),
            AusentesJustificados: registros.Count(r => r.Estado == EstadoAsistencia.AusenteJustificado),
            Ausentes: detalleAusentes);
    }
}
