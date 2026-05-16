namespace PracticaProfesional.Application.Docentes.DTOs;

public record CrearDocenteDto(
    string DNI,
    string Email,
    string Nombre,
    string Apellido,
    string Password,
    string Telefono,
    string Categoria
);
