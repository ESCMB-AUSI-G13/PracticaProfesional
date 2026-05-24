using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Domain.Entities;

public class Alerta
{
    public int Id { get; private set; }
    public TipoAlerta Tipo { get; private set; }
    public string Destinatario { get; private set; } = string.Empty;
    public string Mensaje { get; private set; } = string.Empty;
    public bool Enviada { get; private set; }
    public DateTime FechaCreacion { get; private set; }

    public int? EstudianteId { get; private set; }
    public Estudiante? Estudiante { get; private set; }

    public int? CalendarioAcademicoId { get; private set; }
    public CalendarioAcademico? CalendarioAcademico { get; private set; }

    private Alerta() { }

    public static Alerta Crear(
        TipoAlerta tipo,
        string destinatario,
        string mensaje,
        int? estudianteId = null,
        int? calendarioAcademicoId = null)
    {
        if (string.IsNullOrWhiteSpace(destinatario)) throw new ArgumentException("El destinatario es obligatorio.");
        if (string.IsNullOrWhiteSpace(mensaje)) throw new ArgumentException("El mensaje es obligatorio.");

        return new Alerta
        {
            Tipo = tipo,
            Destinatario = destinatario,
            Mensaje = mensaje,
            Enviada = false,
            FechaCreacion = DateTime.UtcNow,
            EstudianteId = estudianteId,
            CalendarioAcademicoId = calendarioAcademicoId
        };
    }

    public void MarcarEnviada() => Enviada = true;
}
