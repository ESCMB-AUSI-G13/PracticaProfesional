namespace PracticaProfesional.Application.Auditoria.DTOs;

public record AuditoriaLogDto(
    int Id,
    string EntidadTipo,
    string EntidadId,
    string Accion,
    int? EjecutorId,
    string EjecutorEmail,
    string? ValorAnterior,
    string? ValorNuevo,
    DateTime Timestamp
);

public record AuditoriaLogFiltroDto(
    string? EntidadTipo,
    string? Accion,
    string? EjecutorEmail,
    DateTime? FechaDesde,
    DateTime? FechaHasta,
    int Pagina = 1,
    int TamanoPagina = 50
);

public record PaginadoDto<T>(
    IEnumerable<T> Items,
    int TotalRegistros,
    int Pagina,
    int TamanoPagina,
    int TotalPaginas
);
