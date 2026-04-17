namespace PracticaProfesional.Application.Calificaciones.DTOs;

/// <summary>
/// DTO de respuesta tras cargar una nota en una inscripción a examen.
/// </summary>
public record NotaExamenResultDto(
    int InscripcionExamenId,
    int EstudianteId,
    string EstudianteNombreCompleto,
    string EstudianteLegajo,
    int ExamenId,
    string TipoExamen,
    string MateriaNombre,
    decimal NotaValor,
    bool EsAprobado,
    string Estado);
