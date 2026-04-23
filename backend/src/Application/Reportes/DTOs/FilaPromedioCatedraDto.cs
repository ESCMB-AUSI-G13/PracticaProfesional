namespace PracticaProfesional.Application.Reportes.DTOs;

public record FilaPromedioCatedraDto(
    int      EspacioCurricularId,
    string   MateriaNombre,
    string   DocenteNombreCompleto,
    string   Comision,
    int      CursoAnio,
    int      TotalEstudiantes,
    int      TotalConNota,
    int      Aprobados,
    int      Desaprobados,
    decimal? PromedioGeneral,
    decimal  PorcentajeAprobacion
);
