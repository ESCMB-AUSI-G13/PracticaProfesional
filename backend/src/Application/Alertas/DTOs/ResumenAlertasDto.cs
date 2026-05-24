namespace PracticaProfesional.Application.Alertas.DTOs;

public class ResumenAlertasDto
{
    public int AlertasGeneradas { get; init; }
    public int EmailsEnviados { get; init; }
    public IReadOnlyList<string> Detalles { get; init; } = [];
}
