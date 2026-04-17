namespace PracticaProfesional.Application.Calificaciones.DTOs;

/// <summary>
/// Registro del historial de cambios de una nota en particular.
/// Construido a partir de AuditoriaLogs filtrados por entidad InscripcionExamen.
/// </summary>
public record CambioNotaDto(
    int Id,
    string Accion,
    string? ValorAnterior,
    string? ValorNuevo,
    string EjecutorEmail,
    DateTime Timestamp);
