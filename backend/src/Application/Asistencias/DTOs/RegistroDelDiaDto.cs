namespace PracticaProfesional.Application.Asistencias.DTOs;

public record RegistroDelDiaDto(
    int EspacioCurricularId,
    int CursoId,
    int MateriaId,
    string MateriaNombre,
    int AnioLectivo,
    string Comision,
    DateTime Fecha,
    IEnumerable<AsistenciaDetalleDto> Alumnos);
