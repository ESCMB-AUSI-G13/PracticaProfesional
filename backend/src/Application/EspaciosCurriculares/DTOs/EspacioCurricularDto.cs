namespace PracticaProfesional.Application.EspaciosCurriculares.DTOs;

public record EspacioCurricularDto(
    int    Id,
    int    MateriaId,
    string MateriaNombre,
    string MateriaCodigo,
    int    DocenteId,
    string DocenteNombre,
    int    CursoId,
    int    CursoAnio,
    string CursoComision
);
