namespace PracticaProfesional.Application.Reportes.DTOs;

/// <summary>
/// Fila individual del reporte detallado de inasistencias (RR-08).
/// </summary>
public class RegistroInasistenciaDto
{
    public int EstudianteId { get; set; }
    public string Legajo { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Materia { get; set; } = string.Empty;

    /// <summary>Año académico y comisión del curso (ej.: "2024 – A").</summary>
    public string Curso { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }

    /// <summary>"Ausente" o "AusenteJustificado".</summary>
    public string TipoAsistencia { get; set; } = string.Empty;

    /// <summary>Motivo de la justificación (solo cuando aplica).</summary>
    public string? Motivo { get; set; }
}
