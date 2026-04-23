namespace PracticaProfesional.Application.Reportes.DTOs;

public record ReportePromediosCatedraDto(
    DateTime GeneradoEn,
    int?     AnioFiltro,
    IEnumerable<FilaPromedioCatedraDto> Catedras
);
