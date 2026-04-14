using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Domain.Events;

public sealed class EstadoAcademicoChangedEvent(
    int estudianteId,
    CondicionEstudiante condicionAnterior,
    CondicionEstudiante condicionNueva,
    string motivo) : IDomainEvent
{
    public int EstudianteId { get; } = estudianteId;
    public CondicionEstudiante CondicionAnterior { get; } = condicionAnterior;
    public CondicionEstudiante CondicionNueva { get; } = condicionNueva;
    public string Motivo { get; } = motivo;
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
