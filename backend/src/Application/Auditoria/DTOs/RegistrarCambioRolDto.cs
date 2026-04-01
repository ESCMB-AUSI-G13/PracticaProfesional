namespace PracticaProfesional.Application.Auditoria.DTOs;

public record RegistrarCambioRolDto(
    string RolOriginal,
    string RolVista,
    string Accion  // "ACTIVAR" | "RESTAURAR"
);
