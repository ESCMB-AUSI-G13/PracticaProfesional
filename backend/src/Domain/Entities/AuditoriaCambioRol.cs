namespace PracticaProfesional.Domain.Entities;

public class AuditoriaCambioRol
{
    public int Id { get; private set; }
    public int UsuarioId { get; private set; }
    public string RolOriginal { get; private set; } = string.Empty;
    public string RolVista { get; private set; } = string.Empty;
    public string Accion { get; private set; } = string.Empty; // "ACTIVAR" | "RESTAURAR"
    public DateTime Timestamp { get; private set; }

    private AuditoriaCambioRol() { }

    public static AuditoriaCambioRol Registrar(int usuarioId, string rolOriginal, string rolVista, string accion)
        => new()
        {
            UsuarioId = usuarioId,
            RolOriginal = rolOriginal,
            RolVista = rolVista,
            Accion = accion,
            Timestamp = DateTime.UtcNow
        };
}
