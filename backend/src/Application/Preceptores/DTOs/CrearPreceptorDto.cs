namespace PracticaProfesional.Application.Preceptores.DTOs;

public record CrearPreceptorDto(
    string DNI,
    string Legajo,
    string Email,
    string Nombre,
    string Apellido,
    string Password,
    string Telefono,
    string Turno
);
