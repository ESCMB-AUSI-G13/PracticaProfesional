namespace PracticaProfesional.Application.Auth.DTOs;

public record RegistroRequestDto(
    string   DNI,
    string   Email,
    string   Nombre,
    string   Apellido,
    string   Password,
    int      CarreraId,
    int      Anio,
    DateTime FechaDeIngreso
);
