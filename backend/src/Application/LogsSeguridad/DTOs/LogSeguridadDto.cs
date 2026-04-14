namespace PracticaProfesional.Application.LogsSeguridad.DTOs;

public record LogSeguridadDto(
    int Id,
    string Email,
    bool Exitoso,
    string? MotivoFallo,
    string IpOrigen,
    string UserAgent,
    DateTime Timestamp
);

public record LogSeguridadFiltroDto(
    string? Email,
    bool? SoloFallidos,
    DateTime? FechaDesde,
    DateTime? FechaHasta,
    int Pagina = 1,
    int TamanoPagina = 50
);
