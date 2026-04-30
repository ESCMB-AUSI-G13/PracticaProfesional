namespace PracticaProfesional.Application.Inscripciones.DTOs;

public record ComprobanteInscripcionExamenDto(
    int Id,
    string EstudianteNombreCompleto,
    string EstudianteDni,
    string EstudianteLegajo,
    string MateriaCodigo,
    string MateriaNombre,
    string TipoExamen,
    DateTime FechaExamen,
    string Horario,
    string Estado,
    DateTime FechaInscripcion,
    DateTime FechaEmision
);
