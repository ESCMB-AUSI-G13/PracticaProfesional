namespace PracticaProfesional.Application.Usuarios.DTOs;

public record ModificarUsuarioDto(
    string Nombre,
    string Apellido,
    string Email,
    string Rol
);
