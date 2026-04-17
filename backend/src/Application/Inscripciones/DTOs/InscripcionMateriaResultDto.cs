namespace PracticaProfesional.Application.Inscripciones.DTOs;

public record InscripcionMateriaResultDto(
    int Id,
    int EstudianteId,
    int MateriaId,
    string MateriaNombre,
    int CursoId,
    string Estado,
    DateTime FechaInscripcion
);
