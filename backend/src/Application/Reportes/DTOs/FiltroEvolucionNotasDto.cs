using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Reportes.DTOs;

public record FiltroEvolucionNotasDto(
    int?       MateriaId,
    int?       Anio,
    int?       DocenteId,
    int?       Cuatrimestre,
    byte?      AnioCarrera,
    TipoExamen? TipoExamen,
    string     Granularidad = "mensual"   // "mensual" | "cuatrimestral" | "anual"
);
