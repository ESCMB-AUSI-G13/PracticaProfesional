namespace PracticaProfesional.Application.Examenes.DTOs;

public record ExamenDto(
    int      Id,
    int      MateriaId,
    string   MateriaNombre,
    DateTime FechaExamen,
    string   Horario,
    int      Cupo,
    string   TipoExamen
);
