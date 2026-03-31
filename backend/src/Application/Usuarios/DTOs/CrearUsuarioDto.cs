namespace PracticaProfesional.Application.Usuarios.DTOs;

public record CrearUsuarioDto(
    string DNI,
    string Legajo,
    string Email,
    string Nombre,
    string Apellido,
    string Password,
    string Rol
);
