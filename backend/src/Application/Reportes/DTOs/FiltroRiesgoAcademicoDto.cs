namespace PracticaProfesional.Application.Reportes.DTOs;

public record FiltroRiesgoAcademicoDto(
    int?    AnioCohorte,   // null = todas las cohortes
    int?    CarreraId,     // null = todas las carreras
    string? NivelRiesgo    // "Alto" | "Medio" | "Bajo" | null = todos
);

public record FiltroRetencionCohorteDto(
    int? CarreraId,        // null = todas las carreras
    int? AnioCohorte       // null = todos los años
);
