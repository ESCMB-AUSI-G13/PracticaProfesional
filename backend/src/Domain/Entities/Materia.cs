namespace PracticaProfesional.Domain.Entities;

public class Materia
{
    public int Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string Plan { get; private set; } = string.Empty;

    public ICollection<Correlatividad> CorrelativasDependientes { get; private set; } = new List<Correlatividad>();
    public ICollection<Correlatividad> CorrelativasRequisito { get; private set; } = new List<Correlatividad>();

    private Materia() { }

    public static Materia Crear(string codigo, string nombre, string plan)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("El código es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(plan)) throw new ArgumentException("El plan es obligatorio.");

        return new Materia { Codigo = codigo.ToUpperInvariant(), Nombre = nombre, Plan = plan };
    }

    public void Modificar(string nombre, string plan)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(plan)) throw new ArgumentException("El plan es obligatorio.");
        Nombre = nombre;
        Plan = plan;
    }
}
