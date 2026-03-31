namespace PracticaProfesional.Application.Auth.DTOs;

public record AuthResponseDto(
    string Token,
    string Email,
    string NombreCompleto,
    string Rol,
    DateTime Expiracion
);
