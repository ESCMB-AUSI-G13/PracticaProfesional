namespace PracticaProfesional.Application.Reportes.DTOs;

public record FiltroEvolucionNotasDto(
    int? MateriaId,
    int? Anio,
    int? DocenteId
);
