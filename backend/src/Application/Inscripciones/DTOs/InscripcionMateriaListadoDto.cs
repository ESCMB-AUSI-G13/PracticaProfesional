namespace PracticaProfesional.Application.Inscripciones.DTOs;

public record InscripcionMateriaListadoDto(
    int Id,
    int EstudianteId,
    string EstudianteNombre,
    string EstudianteLegajo,
    int CarreraId,
    string CarreraNombre,
    int MateriaId,
    string MateriaCodigo,
    string MateriaNombre,
    int CursoId,
    int CursoAnio,
    int CursoAnioLectivo,
    string CursoComision,
    string Estado,
    DateTime FechaInscripcion
);
