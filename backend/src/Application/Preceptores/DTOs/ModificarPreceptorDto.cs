namespace PracticaProfesional.Application.Preceptores.DTOs;

public record ModificarPreceptorDto(
    string Nombre,
    string Apellido,
    string Email,
    string Telefono,
    string Turno
);
