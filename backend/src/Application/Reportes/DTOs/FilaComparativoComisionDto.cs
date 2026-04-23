namespace PracticaProfesional.Application.Reportes.DTOs;

public record FilaComparativoComisionDto(
    int     CursoAnio,
    string  Comision,
    int     TotalInscriptos,
    int     TotalConNota,
    int     Aprobados,
    int     Desaprobados,
    decimal? PromedioGeneral,
    decimal PorcentajeAprobacion
);
