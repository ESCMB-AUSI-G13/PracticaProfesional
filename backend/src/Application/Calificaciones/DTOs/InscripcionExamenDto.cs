namespace PracticaProfesional.Application.Calificaciones.DTOs;

/// <summary>
/// DTO de lectura de una inscripción a examen.
/// Usado por el docente para ver el listado de alumnos de un examen antes de cargar notas.
/// </summary>
public record InscripcionExamenDto(
    int Id,
    int EstudianteId,
    string EstudianteNombreCompleto,
    string EstudianteLegajo,
    int ExamenId,
    string TipoExamen,
    string MateriaNombre,
    decimal? NotaValor,
    bool? EsAprobado,
    string Estado,
    DateTime FechaInscripcion);
