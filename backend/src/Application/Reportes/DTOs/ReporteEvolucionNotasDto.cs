namespace PracticaProfesional.Application.Reportes.DTOs;

public record ReporteEvolucionNotasDto(
    DateTime GeneradoEn,
    string?  MateriaNombre,
    int?     AnioFiltro,
    IEnumerable<PuntoEvolucionNotaDto> Evolucion
);
