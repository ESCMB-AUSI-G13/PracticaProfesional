namespace PracticaProfesional.Application.Usuarios.DTOs;

public record CrearUsuarioDto(
    string DNI,
    string Email,
    string Nombre,
    string Apellido,
    string Password,
    string Rol
);
