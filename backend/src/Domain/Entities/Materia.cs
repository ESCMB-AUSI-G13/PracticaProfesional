namespace PracticaProfesional.Domain.Entities;

public class Materia
{
    public int Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public int CarreraId { get; private set; }
    public byte Anio { get; private set; }
    public Carrera Carrera { get; private set; } = null!;

    public ICollection<Correlatividad> CorrelativasDependientes { get; private set; } = new List<Correlatividad>();
    public ICollection<Correlatividad> CorrelativasRequisito { get; private set; } = new List<Correlatividad>();

    private Materia() { }

    public static Materia Crear(string codigo, string nombre, int carreraId, byte anio)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("El código es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.");
        if (carreraId <= 0) throw new ArgumentException("La carrera es obligatoria.");
        if (anio < 1 || anio > 6) throw new ArgumentException("El año debe estar entre 1 y 6.");

        return new Materia { Codigo = codigo.ToUpperInvariant(), Nombre = nombre, CarreraId = carreraId, Anio = anio };
    }

    public void Modificar(string nombre, int carreraId, byte anio)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.");
        if (carreraId <= 0) throw new ArgumentException("La carrera es obligatoria.");
        if (anio < 1 || anio > 6) throw new ArgumentException("El año debe estar entre 1 y 6.");
        Nombre = nombre;
        CarreraId = carreraId;
        Anio = anio;
    }
}
