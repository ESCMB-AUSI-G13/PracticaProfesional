namespace PracticaProfesional.Application.MiHistorial.DTOs;

public record ParcialDto(
    string   FechaExamen,
    decimal? Nota,
    string   Estado
);

public record HistorialMateriaDto(
    int      MateriaId,
    string   MateriaCodigo,
    string   MateriaNombre,
    IEnumerable<ParcialDto> Parciales,
    decimal? NotaFinal,
    string?  EstadoFinal,
    string?  Condicion,
    int?     Anio
);

public record MiHistorialDto(
    IEnumerable<HistorialMateriaDto> Materias,
    decimal? PromedioGeneral
);
