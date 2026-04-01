namespace PracticaProfesional.Application.Auth.DTOs;

public record RegistroRequestDto(
    string DNI,
    string Legajo,
    string Email,
    string Nombre,
    string Apellido,
    string Password,
    string Rol  // Solo "Estudiante" o "Docente"
);
