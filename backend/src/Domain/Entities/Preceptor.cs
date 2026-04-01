namespace PracticaProfesional.Domain.Entities;

public class Preceptor
{
    public int Id { get; private set; }
    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public string Telefono { get; private set; } = string.Empty;
    public string Turno { get; private set; } = string.Empty;

    private Preceptor() { }

    public static Preceptor Crear(int usuarioId, string telefono, string turno)
    {
        if (string.IsNullOrWhiteSpace(telefono)) throw new ArgumentException("El teléfono es obligatorio.");
        if (string.IsNullOrWhiteSpace(turno)) throw new ArgumentException("El turno es obligatorio.");

        return new Preceptor
        {
            UsuarioId = usuarioId,
            Telefono = telefono.Trim(),
            Turno = turno.Trim()
        };
    }

    public void Modificar(string telefono, string turno)
    {
        if (string.IsNullOrWhiteSpace(telefono)) throw new ArgumentException("El teléfono es obligatorio.");
        if (string.IsNullOrWhiteSpace(turno)) throw new ArgumentException("El turno es obligatorio.");

        Telefono = telefono.Trim();
        Turno = turno.Trim();
    }
}
