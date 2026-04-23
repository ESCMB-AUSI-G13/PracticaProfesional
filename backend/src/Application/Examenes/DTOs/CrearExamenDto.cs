namespace PracticaProfesional.Application.Examenes.DTOs;

public record CrearExamenDto(
    int      MateriaId,
    DateTime FechaExamen,
    string   Horario,
    int      Cupo,
    string   TipoExamen
);
