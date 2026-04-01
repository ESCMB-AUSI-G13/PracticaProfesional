namespace PracticaProfesional.Application.Docentes.DTOs;

public record ModificarDocenteDto(
    string Nombre,
    string Apellido,
    string Email,
    string Telefono,
    string Categoria
);
