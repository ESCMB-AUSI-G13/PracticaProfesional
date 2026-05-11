namespace PracticaProfesional.Application.Asistencias.DTOs;

public record ResumenAsistenciasDto(
    DateTime Fecha,
    string MateriaNombre,
    string CursoComision,
    int AnioLectivo,
    int TotalAlumnos,
    int Presentes,
    int AusentesInjustificados,
    int AusentesJustificados,
    IEnumerable<DetalleAusenciaDto> Ausentes);

public record DetalleAusenciaDto(
    string NombreCompleto,
    string Legajo,
    string TipoAusencia,
    string? Motivo);
