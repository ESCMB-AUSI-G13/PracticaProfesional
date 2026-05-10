namespace PracticaProfesional.Application.EspaciosCurriculares.DTOs;

public record EspacioCurricularDto(
    int    Id,
    int    MateriaId,
    string MateriaNombre,
    string MateriaCodigo,
    int    MateriaAnio,
    int    CarreraId,
    string CarreraNombre,
    int    DocenteId,
    string DocenteNombre,
    int    CursoId,
    int    CursoAnio,
    int    CursoAnioLectivo,
    string CursoComision
);
