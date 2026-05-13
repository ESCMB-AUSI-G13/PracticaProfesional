namespace PracticaProfesional.Application.Reportes.DTOs;

public record DetalleCarreraEvolucionDto(
    int      CarreraId,
    string   CarreraNombre,
    decimal? Promedio,
    decimal  PorcentajeAprobacion,
    int      TotalEvaluados
);
