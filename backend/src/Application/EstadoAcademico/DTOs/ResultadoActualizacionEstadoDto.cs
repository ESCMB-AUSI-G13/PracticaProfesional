namespace PracticaProfesional.Application.EstadoAcademico.DTOs;

/// <summary>
/// Resultado de la evaluación automática del estado académico.
/// </summary>
public class ResultadoActualizacionEstadoDto
{
    public int EstudianteId { get; set; }

    /// <summary>Condición antes de la evaluación.</summary>
    public string CondicionAnterior { get; set; } = string.Empty;

    /// <summary>Condición resultante (igual a la anterior si no hubo transición).</summary>
    public string CondicionNueva { get; set; } = string.Empty;

    /// <summary>Indica si se produjo una transición de estado.</summary>
    public bool HuboTransicion { get; set; }

    /// <summary>Descripción del criterio que motivó el cambio (o la ausencia de él).</summary>
    public string Motivo { get; set; } = string.Empty;
}
