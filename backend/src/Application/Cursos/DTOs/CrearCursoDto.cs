namespace PracticaProfesional.Application.Cursos.DTOs;

public record CrearCursoDto(
    int    Anio,
    int    AnioLectivo,
    string Comision,
    int    Cupo,
    int    PreceptorId,
    int    CarreraId
);
