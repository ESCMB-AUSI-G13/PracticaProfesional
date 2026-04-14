using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Domain.Entities;

public class Examen
{
    public int Id { get; private set; }
    public int MateriaId { get; private set; }
    public Materia Materia { get; private set; } = null!;
    public DateTime FechaExamen { get; private set; }
    public string Horario { get; private set; } = string.Empty;
    public int Cupo { get; private set; }
    public TipoExamen TipoExamen { get; private set; }

    private Examen() { }

    public static Examen Crear(int materiaId, DateTime fechaExamen, string horario, int cupo, TipoExamen tipo)
    {
        if (fechaExamen < DateTime.UtcNow.Date) throw new ArgumentException("La fecha del examen no puede ser en el pasado.");
        if (string.IsNullOrWhiteSpace(horario)) throw new ArgumentException("El horario es obligatorio.");
        if (cupo <= 0) throw new ArgumentException("El cupo debe ser mayor a cero.");

        return new Examen
        {
            MateriaId = materiaId,
            FechaExamen = fechaExamen.Date,
            Horario = horario,
            Cupo = cupo,
            TipoExamen = tipo
        };
    }

    public void Modificar(DateTime fechaExamen, string horario, int cupo)
    {
        if (string.IsNullOrWhiteSpace(horario)) throw new ArgumentException("El horario es obligatorio.");
        if (cupo <= 0) throw new ArgumentException("El cupo debe ser mayor a cero.");
        FechaExamen = fechaExamen.Date;
        Horario = horario;
        Cupo = cupo;
    }
}
