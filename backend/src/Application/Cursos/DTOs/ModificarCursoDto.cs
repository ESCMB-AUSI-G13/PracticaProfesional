namespace PracticaProfesional.Application.Cursos.DTOs;

public record ModificarCursoDto(
    string Comision,
    int    Cupo,
    // Ver CrearCursoDto.PreceptorUsuarioId — mismo motivo (Tier 7 #32).
    int    PreceptorUsuarioId
);
