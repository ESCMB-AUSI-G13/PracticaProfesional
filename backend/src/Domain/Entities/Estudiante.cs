using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Domain.Entities;

public class Estudiante
{
    public int Id { get; private set; }
    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public int Anio { get; private set; }
    public CondicionEstudiante Condicion { get; private set; }
    public DateTime FechaDeIngreso { get; private set; }

    private Estudiante() { }

    public static Estudiante Crear(int usuarioId, int anio, DateTime fechaDeIngreso)
    {
        if (anio < 1 || anio > 6) throw new ArgumentException("El año debe estar entre 1 y 6.");

        return new Estudiante
        {
            UsuarioId = usuarioId,
            Anio = anio,
            Condicion = CondicionEstudiante.Regular,
            FechaDeIngreso = fechaDeIngreso.Date
        };
    }

    public void Modificar(int anio, CondicionEstudiante condicion)
    {
        if (anio < 1 || anio > 6) throw new ArgumentException("El año debe estar entre 1 y 6.");

        Anio = anio;
        Condicion = condicion;
    }
}
