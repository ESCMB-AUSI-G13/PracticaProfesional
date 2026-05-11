namespace PracticaProfesional.Application.Asistencias.DTOs;

public record EspacioAsistenciaDto(
    int EspacioCurricularId,
    int CursoId,
    int MateriaId,
    string MateriaNombre,
    int AnioLectivo,
    string Comision);
