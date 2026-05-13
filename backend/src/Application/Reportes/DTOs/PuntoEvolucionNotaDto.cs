namespace PracticaProfesional.Application.Reportes.DTOs;

public record PuntoEvolucionNotaDto(
    string   Periodo,
    int      TotalEvaluados,
    int      Aprobados,
    int      Desaprobados,
    decimal? PromedioGeneral,
    decimal  PorcentajeAprobacion,
    IEnumerable<DetalleCarreraEvolucionDto> PorCarrera
);
