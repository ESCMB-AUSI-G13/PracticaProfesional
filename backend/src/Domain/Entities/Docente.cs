namespace PracticaProfesional.Domain.Entities;

public class Docente
{
    public int Id { get; private set; }
    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public string Telefono { get; private set; } = string.Empty;
    public string Categoria { get; private set; } = string.Empty;

    private Docente() { }

    public static Docente Crear(int usuarioId, string telefono, string categoria)
    {
        if (string.IsNullOrWhiteSpace(telefono)) throw new ArgumentException("El teléfono es obligatorio.");
        if (string.IsNullOrWhiteSpace(categoria)) throw new ArgumentException("La categoría es obligatoria.");

        return new Docente
        {
            UsuarioId = usuarioId,
            Telefono = telefono.Trim(),
            Categoria = categoria.Trim()
        };
    }

    public void Modificar(string telefono, string categoria)
    {
        if (string.IsNullOrWhiteSpace(telefono)) throw new ArgumentException("El teléfono es obligatorio.");
        if (string.IsNullOrWhiteSpace(categoria)) throw new ArgumentException("La categoría es obligatoria.");

        Telefono = telefono.Trim();
        Categoria = categoria.Trim();
    }
}
