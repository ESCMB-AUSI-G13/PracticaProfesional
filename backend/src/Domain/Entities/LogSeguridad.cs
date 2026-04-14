namespace PracticaProfesional.Domain.Entities;

public class LogSeguridad
{
    public int Id { get; private set; }

    /// <summary>Email ingresado en el intento (puede no existir en el sistema).</summary>
    public string Email { get; private set; } = string.Empty;

    public bool Exitoso { get; private set; }

    /// <summary>Razón del fallo. Null si fue exitoso.</summary>
    public string? MotivoFallo { get; private set; }

    public string IpOrigen { get; private set; } = string.Empty;

    public string UserAgent { get; private set; } = string.Empty;

    public DateTime Timestamp { get; private set; }

    private LogSeguridad() { }

    public static LogSeguridad Registrar(
        string email,
        bool exitoso,
        string ipOrigen,
        string userAgent,
        string? motivoFallo = null)
        => new()
        {
            Email      = email,
            Exitoso    = exitoso,
            MotivoFallo = exitoso ? null : (motivoFallo ?? "Credenciales inválidas"),
            IpOrigen   = ipOrigen,
            UserAgent  = userAgent,
            Timestamp  = DateTime.UtcNow
        };
}
