using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Domain.Entities;

public class Notificacion
{
    public int Id { get; private set; }
    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public string Titulo { get; private set; } = string.Empty;
    public string Mensaje { get; private set; } = string.Empty;
    public bool Leida { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public TipoAlerta? Tipo { get; private set; }

    private Notificacion() { }

    public static Notificacion Crear(int usuarioId, string titulo, string mensaje, TipoAlerta? tipo = null)
    {
        if (string.IsNullOrWhiteSpace(titulo)) throw new ArgumentException("El título es obligatorio.");
        if (string.IsNullOrWhiteSpace(mensaje)) throw new ArgumentException("El mensaje es obligatorio.");

        return new Notificacion
        {
            UsuarioId = usuarioId,
            Titulo = titulo,
            Mensaje = mensaje,
            Leida = false,
            FechaCreacion = DateTime.UtcNow,
            Tipo = tipo
        };
    }

    public void MarcarLeida() => Leida = true;
}
