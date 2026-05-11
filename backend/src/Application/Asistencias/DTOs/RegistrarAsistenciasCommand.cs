namespace PracticaProfesional.Application.Asistencias.DTOs;

public record RegistrarAsistenciasCommand(
    int EspacioCurricularId,
    DateTime Fecha,
    IEnumerable<AusenciaItemCommand> Ausentes);

public record AusenciaItemCommand(
    int EstudianteId,
    string TipoAusencia,   // "Injustificada" | "Justificada"
    string? Motivo);
