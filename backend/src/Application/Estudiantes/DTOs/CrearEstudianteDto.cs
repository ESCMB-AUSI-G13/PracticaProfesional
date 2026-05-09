namespace PracticaProfesional.Application.Estudiantes.DTOs;

public record CrearEstudianteDto(
    string   DNI,
    string   Legajo,
    string   Email,
    string   Nombre,
    string   Apellido,
    string   Password,
    int      Anio,
    int      CarreraId,
    DateTime FechaDeIngreso
);
