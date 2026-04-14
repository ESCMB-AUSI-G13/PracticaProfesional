namespace PracticaProfesional.Domain.Entities;

public class Alerta
{
    public int Id { get; private set; }
    public int Anio { get; private set; }
    public string Comision { get; private set; } = string.Empty;
    public string EstadoFinal { get; private set; } = string.Empty;
    public decimal? NotaFinal { get; private set; }
    public string Condicion { get; private set; } = string.Empty;
    public bool Enviada { get; private set; }
    public DateTime FechaCreacion { get; private set; }

    public int? InscripcionExamenId { get; private set; }
    public InscripcionExamen? InscripcionExamen { get; private set; }
    public int? InscripcionMateriaId { get; private set; }
    public InscripcionMateria? InscripcionMateria { get; private set; }
    public int? ExamenId { get; private set; }
    public Examen? Examen { get; private set; }

    private Alerta() { }

    public static Alerta Crear(
        int anio,
        string comision,
        string estadoFinal,
        decimal? notaFinal,
        string condicion,
        int? inscripcionExamenId = null,
        int? inscripcionMateriaId = null,
        int? examenId = null)
    {
        if (string.IsNullOrWhiteSpace(comision)) throw new ArgumentException("La comisión es obligatoria.");
        if (string.IsNullOrWhiteSpace(estadoFinal)) throw new ArgumentException("El estado final es obligatorio.");

        return new Alerta
        {
            Anio = anio,
            Comision = comision,
            EstadoFinal = estadoFinal,
            NotaFinal = notaFinal,
            Condicion = condicion,
            Enviada = false,
            FechaCreacion = DateTime.UtcNow,
            InscripcionExamenId = inscripcionExamenId,
            InscripcionMateriaId = inscripcionMateriaId,
            ExamenId = examenId
        };
    }

    public void MarcarEnviada() => Enviada = true;
}
