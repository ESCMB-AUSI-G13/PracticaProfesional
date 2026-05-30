namespace PracticaProfesional.Application.Cursos.DTOs;

public record CursoDto(
    int    Id,
    int    Anio,
    int    AnioLectivo,
    string Comision,
    int    Cupo,
    string Estado,
    int    PreceptorId,
    string PreceptorNombre,
    int    CarreraId
);
