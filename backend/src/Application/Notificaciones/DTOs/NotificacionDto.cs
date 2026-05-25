namespace PracticaProfesional.Application.Notificaciones.DTOs;

public class NotificacionDto
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string Mensaje { get; init; } = string.Empty;
    public bool Leida { get; init; }
    public DateTime FechaCreacion { get; init; }
    public string? Tipo { get; init; }
}
