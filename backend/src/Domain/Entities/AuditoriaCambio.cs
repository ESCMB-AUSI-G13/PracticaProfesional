namespace PracticaProfesional.Domain.Entities;

/// <summary>
/// Registro inmutable de cambios en Notas e Inscripciones (CU-06).
/// </summary>
public class AuditoriaCambio
{
    public int Id { get; private set; }
    public string TablaAfectada { get; private set; } = string.Empty;
    public string RegistroAfectado { get; private set; } = string.Empty;
    public string Accion { get; private set; } = string.Empty;
    public DateTime FechaCambio { get; private set; }
    public string? ValorAnterior { get; private set; }
    public string? ValorNuevo { get; private set; }
    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;

    public int? ExamenId { get; private set; }
    public Examen? Examen { get; private set; }
    public int? CalendarioId { get; private set; }
    public CalendarioAcademico? Calendario { get; private set; }
    public int? InscripcionExamenId { get; private set; }
    public InscripcionExamen? InscripcionExamen { get; private set; }
    public int? InscripcionMateriaId { get; private set; }
    public InscripcionMateria? InscripcionMateria { get; private set; }
    public int? EncuestaId { get; private set; }
    public Encuesta? Encuesta { get; private set; }

    private AuditoriaCambio() { }

    public static AuditoriaCambio Registrar(
        string tablaAfectada,
        string registroAfectado,
        string accion,
        int usuarioId,
        string? valorAnterior,
        string? valorNuevo,
        int? examenId = null,
        int? calendarioId = null,
        int? inscripcionExamenId = null,
        int? inscripcionMateriaId = null,
        int? encuestaId = null)
    {
        if (string.IsNullOrWhiteSpace(tablaAfectada)) throw new ArgumentException("La tabla afectada es obligatoria.");
        if (string.IsNullOrWhiteSpace(registroAfectado)) throw new ArgumentException("El registro afectado es obligatorio.");
        if (string.IsNullOrWhiteSpace(accion)) throw new ArgumentException("La acción es obligatoria.");

        return new AuditoriaCambio
        {
            TablaAfectada = tablaAfectada,
            RegistroAfectado = registroAfectado,
            Accion = accion,
            FechaCambio = DateTime.UtcNow,
            UsuarioId = usuarioId,
            ValorAnterior = valorAnterior,
            ValorNuevo = valorNuevo,
            ExamenId = examenId,
            CalendarioId = calendarioId,
            InscripcionExamenId = inscripcionExamenId,
            InscripcionMateriaId = inscripcionMateriaId,
            EncuestaId = encuestaId
        };
    }
}
