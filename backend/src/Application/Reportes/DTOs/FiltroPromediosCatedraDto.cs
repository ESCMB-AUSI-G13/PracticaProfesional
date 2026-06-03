namespace PracticaProfesional.Application.Reportes.DTOs;

public record FiltroPromediosCatedraDto(
    int? DocenteId,
    int? Anio,
    int? CursoId,
    int? CarreraId
);
