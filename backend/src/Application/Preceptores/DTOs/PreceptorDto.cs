namespace PracticaProfesional.Application.Preceptores.DTOs;

public record PreceptorDto(
    int Id,
    int UsuarioId,
    string DNI,
    string Legajo,
    string Email,
    string Nombre,
    string Apellido,
    string Telefono,
    string Turno,
    bool Activo,
    DateTime FechaCreacion
);
