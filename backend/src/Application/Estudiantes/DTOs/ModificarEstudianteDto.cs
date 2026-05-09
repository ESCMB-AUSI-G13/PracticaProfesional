namespace PracticaProfesional.Application.Estudiantes.DTOs;

public record ModificarEstudianteDto(
    string Nombre,
    string Apellido,
    string Email,
    int    Anio,
    int    CarreraId,
    string Condicion
);
