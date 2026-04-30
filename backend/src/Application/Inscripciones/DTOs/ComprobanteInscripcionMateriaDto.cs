namespace PracticaProfesional.Application.Inscripciones.DTOs;

public record ComprobanteInscripcionMateriaDto(
    int Id,
    string EstudianteNombreCompleto,
    string EstudianteDni,
    string EstudianteLegajo,
    string MateriaCodigo,
    string MateriaNombre,
    string MateriaPlan,
    int CursoAnioLectivo,
    string CursoComision,
    string Estado,
    DateTime FechaInscripcion,
    DateTime FechaEmision
);
