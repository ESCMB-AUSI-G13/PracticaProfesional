using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.ValueObjects;

namespace PracticaProfesional.Domain.Entities;

public class InscripcionExamen
{
    public int Id { get; private set; }
    public int EstudianteId { get; private set; }
    public Estudiante Estudiante { get; private set; } = null!;
    public int ExamenId { get; private set; }
    public Examen Examen { get; private set; } = null!;
    public EstadoInscripcion Estado { get; private set; }
    // Nota cargada después de rendir (null hasta que se cargue)
    public decimal? NotaValor { get; private set; }
    public DateTime FechaInscripcion { get; private set; }

    private InscripcionExamen() { }

    public static InscripcionExamen Crear(int estudianteId, int examenId)
    {
        return new InscripcionExamen
        {
            EstudianteId = estudianteId,
            ExamenId = examenId,
            Estado = EstadoInscripcion.Activa,
            FechaInscripcion = DateTime.UtcNow
        };
    }

    public void CargarNota(Nota nota)
    {
        NotaValor = nota.Valor;
        Estado = nota.EsAprobado ? EstadoInscripcion.Aprobada : EstadoInscripcion.Desaprobada;
    }

    public void DarDeBaja() => Estado = EstadoInscripcion.Baja;
}
