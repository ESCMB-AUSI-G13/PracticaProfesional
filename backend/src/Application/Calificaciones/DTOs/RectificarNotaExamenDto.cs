namespace PracticaProfesional.Application.Calificaciones.DTOs;

/// <summary>
/// DTO de entrada para rectificar una nota ya cargada.
/// Solo aplica cuando la inscripción está en estado Aprobada o Desaprobada.
/// </summary>
public record RectificarNotaExamenDto(
    int InscripcionExamenId,
    decimal NuevaNota,
    string Motivo);
