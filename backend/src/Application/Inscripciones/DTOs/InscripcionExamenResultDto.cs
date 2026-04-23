namespace PracticaProfesional.Application.Inscripciones.DTOs;

public record InscripcionExamenResultDto(
    int      Id,
    int      EstudianteId,
    string   EstudianteNombre,
    int      ExamenId,
    string   MateriaNombre,
    string   TipoExamen,
    DateTime FechaExamen,
    string   Estado
);
