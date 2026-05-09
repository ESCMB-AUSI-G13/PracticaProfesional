namespace PracticaProfesional.Application.Estudiantes.DTOs;

public record EstudianteDto(
    int      Id,
    int      UsuarioId,
    string   DNI,
    string   Legajo,
    string   Email,
    string   Nombre,
    string   Apellido,
    int      Anio,
    int      CarreraId,
    string   CarreraNombre,
    string   Condicion,
    DateTime FechaDeIngreso,
    bool     Activo,
    DateTime FechaCreacion
);
