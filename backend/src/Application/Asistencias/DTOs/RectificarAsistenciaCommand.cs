namespace PracticaProfesional.Application.Asistencias.DTOs;

public record RectificarAsistenciaCommand(
    int EspacioCurricularId,
    DateTime Fecha,
    IEnumerable<CambioAsistenciaItem> Cambios);

public record CambioAsistenciaItem(
    int AsistenciaId,
    string NuevoEstado,
    string? Motivo);
