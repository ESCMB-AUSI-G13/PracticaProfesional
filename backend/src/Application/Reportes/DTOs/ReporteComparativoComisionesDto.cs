namespace PracticaProfesional.Application.Reportes.DTOs;

public record ReporteComparativoComisionesDto(
    DateTime GeneradoEn,
    string?  MateriaNombre,
    int?     AnioFiltro,
    IEnumerable<FilaComparativoComisionDto> Comisiones
);
