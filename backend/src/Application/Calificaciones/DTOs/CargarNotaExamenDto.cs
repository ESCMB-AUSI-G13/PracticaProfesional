namespace PracticaProfesional.Application.Calificaciones.DTOs;

/// <summary>
/// DTO de entrada para cargar la nota de un estudiante en un examen.
/// El Id de la inscripción al examen se recibe por ruta; la nota por body.
/// </summary>
public record CargarNotaExamenDto(int InscripcionExamenId, decimal Nota);
