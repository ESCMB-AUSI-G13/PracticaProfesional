namespace PracticaProfesional.Application.Alertas.DTOs;

public class AlertaDto
{
    public int Id { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public string Destinatario { get; init; } = string.Empty;
    public string Mensaje { get; init; } = string.Empty;
    public bool Enviada { get; init; }
    public DateTime FechaCreacion { get; init; }
    public int? EstudianteId { get; init; }
    public string? NombreEstudiante { get; init; }
    public int? CalendarioAcademicoId { get; init; }
    public string? NombreEvento { get; init; }
}
