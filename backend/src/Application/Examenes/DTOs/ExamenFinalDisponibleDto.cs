namespace PracticaProfesional.Application.Examenes.DTOs;

public record ExamenFinalDisponibleDto(
    int    Id,
    int    MateriaId,
    string MateriaNombre,
    string MateriaCodigo,
    string FechaExamen,
    string Horario,
    int    Cupo,
    string TipoExamen,
    bool   YaInscripto
);
