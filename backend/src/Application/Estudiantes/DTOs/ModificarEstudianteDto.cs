namespace PracticaProfesional.Application.Estudiantes.DTOs;

public record ModificarEstudianteDto(
    string Nombre,
    string Apellido,
    string Email,
    int Anio,
    string Condicion
);
