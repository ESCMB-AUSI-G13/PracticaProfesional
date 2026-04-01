namespace PracticaProfesional.Application.Docentes.DTOs;

public record DocenteDto(
    int Id,
    int UsuarioId,
    string DNI,
    string Legajo,
    string Email,
    string Nombre,
    string Apellido,
    string Telefono,
    string Categoria,
    bool Activo,
    DateTime FechaCreacion
);
