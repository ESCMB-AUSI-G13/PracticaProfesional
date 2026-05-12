namespace PracticaProfesional.Application.Asistencias.DTOs;

public record AsistenciaDetalleDto(
    int AsistenciaId,
    int EstudianteId,
    string NombreCompleto,
    string Legajo,
    string Estado,
    string? Motivo);
