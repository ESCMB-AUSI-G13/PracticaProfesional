namespace PracticaProfesional.Application.Materias.DTOs;

public record MateriaDto(
    int    Id,
    string Codigo,
    string Nombre,
    int    CarreraId,
    string CarreraNombre,
    byte   Anio
);
