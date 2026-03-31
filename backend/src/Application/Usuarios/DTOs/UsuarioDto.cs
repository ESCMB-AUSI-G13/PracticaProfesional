namespace PracticaProfesional.Application.Usuarios.DTOs;

public record UsuarioDto(
    int Id,
    string DNI,
    string Legajo,
    string Email,
    string Nombre,
    string Apellido,
    string Rol,
    bool Activo,
    DateTime FechaCreacion
);
