namespace PracticaProfesional.Domain.Entities;

public class AuditoriaLog
{
    public int Id { get; private set; }

    /// <summary>Tipo de entidad afectada: "Usuario", "Docente", "Preceptor", "Estudiante".</summary>
    public string EntidadTipo { get; private set; } = string.Empty;

    /// <summary>ID de la entidad afectada (como string para generalidad).</summary>
    public string EntidadId { get; private set; } = string.Empty;

    /// <summary>Acción ejecutada: "CREAR", "MODIFICAR", "DESACTIVAR", "REACTIVAR".</summary>
    public string Accion { get; private set; } = string.Empty;

    public int? EjecutorId { get; private set; }
    public string EjecutorEmail { get; private set; } = string.Empty;

    /// <summary>Estado anterior de la entidad en formato JSON. Null si es creación.</summary>
    public string? ValorAnterior { get; private set; }

    /// <summary>Estado nuevo de la entidad en formato JSON. Null si es baja definitiva.</summary>
    public string? ValorNuevo { get; private set; }

    public DateTime Timestamp { get; private set; }

    private AuditoriaLog() { }

    public static AuditoriaLog Registrar(
        string entidadTipo,
        string entidadId,
        string accion,
        int? ejecutorId,
        string ejecutorEmail,
        string? valorAnterior,
        string? valorNuevo)
        => new()
        {
            EntidadTipo = entidadTipo,
            EntidadId = entidadId,
            Accion = accion,
            EjecutorId = ejecutorId,
            EjecutorEmail = ejecutorEmail,
            ValorAnterior = valorAnterior,
            ValorNuevo = valorNuevo,
            Timestamp = DateTime.UtcNow
        };
}
