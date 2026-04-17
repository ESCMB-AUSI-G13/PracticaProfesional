namespace PracticaProfesional.Application.Reportes.DTOs;

/// <summary>
/// Resumen de asistencia de un estudiante en una materia/curso específico (RR-09).
/// </summary>
public class ResumenAsistenciaMateriaDto
{
    public int MateriaId { get; set; }
    public string Materia { get; set; } = string.Empty;

    /// <summary>Año académico y comisión del curso (ej.: "2024 – A").</summary>
    public string Curso { get; set; } = string.Empty;

    public int TotalClases { get; set; }
    public int Presentes { get; set; }
    public int AusentesJustificados { get; set; }
    public int AusentesInjustificados { get; set; }

    /// <summary>
    /// Porcentaje de presencia efectiva = (Presentes + AusentesJustificados) / TotalClases.
    /// </summary>
    public decimal PorcentajePresencia { get; set; }

    /// <summary>
    /// Porcentaje de ausencias injustificadas sobre el total de clases.
    /// </summary>
    public decimal PorcentajeAusencias { get; set; }

    /// <summary>
    /// Indica que el estudiante superó el 20 % de ausencias injustificadas
    /// (zona de alerta antes del umbral de pérdida de regularidad del 25 %).
    /// </summary>
    public bool EnRiesgoRegularidad { get; set; }

    /// <summary>
    /// Indica que el estudiante superó el 25 % de ausencias injustificadas
    /// y ha perdido (o debería perder) la regularidad en esta materia.
    /// </summary>
    public bool PerdioRegularidad { get; set; }
}
