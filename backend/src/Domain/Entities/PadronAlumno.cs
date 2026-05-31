namespace PracticaProfesional.Domain.Entities;

public class PadronAlumno
{
    public string DNI { get; private set; } = string.Empty;
    public DateTime FechaCarga { get; private set; }

    private PadronAlumno() { }

    public static PadronAlumno Crear(string dni)
    {
        if (string.IsNullOrWhiteSpace(dni))
            throw new ArgumentException("El DNI es obligatorio.");

        var dnilimpio = dni.Trim();

        if (!dnilimpio.All(char.IsDigit))
            throw new ArgumentException("El DNI solo puede contener dígitos.");

        if (dnilimpio.Length < 7 || dnilimpio.Length > 10)
            throw new ArgumentException("El DNI debe tener entre 7 y 10 dígitos.");

        return new PadronAlumno
        {
            DNI = dnilimpio,
            FechaCarga = DateTime.UtcNow
        };
    }
}
