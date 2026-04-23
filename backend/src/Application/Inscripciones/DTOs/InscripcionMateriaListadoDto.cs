namespace PracticaProfesional.Application.Inscripciones.DTOs;

public record InscripcionMateriaListadoDto(
    int Id,
    int EstudianteId,
    string EstudianteNombre,
    int MateriaId,
    string MateriaCodigo,
    string MateriaNombre,
    int CursoId,
    int CursoAnio,
    string CursoComision,
    string Estado,
    DateTime FechaInscripcion
);
