namespace PracticaProfesional.Application.EspaciosCurriculares.DTOs;

public record CrearEspacioCurricularDto(
    int MateriaId,
    int UsuarioDocenteId,
    int CursoId
);
