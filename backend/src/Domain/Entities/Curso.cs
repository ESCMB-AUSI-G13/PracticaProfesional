using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Domain.Entities;

public class Curso
{
    public int Id { get; private set; }
    public int Anio { get; private set; }
    public string Comision { get; private set; } = string.Empty;
    public int Cupo { get; private set; }
    public EstadoCurso Estado { get; private set; }
    public int PreceptorId { get; private set; }
    public Preceptor Preceptor { get; private set; } = null!;

    private Curso() { }

    public static Curso Crear(int anio, string comision, int cupo, int preceptorId)
    {
        if (anio < 2000 || anio > 2100) throw new ArgumentException("El año académico no es válido.");
        if (string.IsNullOrWhiteSpace(comision)) throw new ArgumentException("La comisión es obligatoria.");
        if (cupo <= 0) throw new ArgumentException("El cupo debe ser mayor a cero.");

        return new Curso
        {
            Anio = anio,
            Comision = comision.ToUpperInvariant(),
            Cupo = cupo,
            Estado = EstadoCurso.Activo,
            PreceptorId = preceptorId
        };
    }

    public void Cerrar() => Estado = EstadoCurso.Cerrado;
    public void Suspender() => Estado = EstadoCurso.Suspendido;
    public void Reactivar() => Estado = EstadoCurso.Activo;

    public void Modificar(string comision, int cupo)
    {
        if (string.IsNullOrWhiteSpace(comision)) throw new ArgumentException("La comisión es obligatoria.");
        if (cupo <= 0) throw new ArgumentException("El cupo debe ser mayor a cero.");
        Comision = comision.ToUpperInvariant();
        Cupo = cupo;
    }
}
