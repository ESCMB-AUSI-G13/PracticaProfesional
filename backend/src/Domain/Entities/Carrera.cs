namespace PracticaProfesional.Domain.Entities;

public class Carrera
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Resolucion { get; private set; } = string.Empty;

    public ICollection<Materia> Materias { get; private set; } = new List<Materia>();
    public ICollection<Estudiante> Estudiantes { get; private set; } = new List<Estudiante>();

    private Carrera() { }

    public static Carrera Crear(string nombre, string resolucion)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(resolucion)) throw new ArgumentException("La resolución es obligatoria.");
        return new Carrera { Nombre = nombre.Trim(), Resolucion = resolucion.Trim() };
    }
}
