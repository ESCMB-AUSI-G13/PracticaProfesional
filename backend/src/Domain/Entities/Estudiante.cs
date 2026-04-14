using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Events;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Domain.Entities;

public class Estudiante : AggregateRoot
{
    // ── Transiciones válidas por estado ─────────────────────────────────────
    private static readonly Dictionary<CondicionEstudiante, IReadOnlySet<CondicionEstudiante>> _transicionesValidas =
        new()
        {
            [CondicionEstudiante.Regular]      = new HashSet<CondicionEstudiante> { CondicionEstudiante.Libre, CondicionEstudiante.Promocional, CondicionEstudiante.Egresado, CondicionEstudiante.Desertor },
            [CondicionEstudiante.Libre]        = new HashSet<CondicionEstudiante> { CondicionEstudiante.Regular, CondicionEstudiante.Egresado, CondicionEstudiante.Desertor },
            [CondicionEstudiante.Promocional]  = new HashSet<CondicionEstudiante> { CondicionEstudiante.Regular, CondicionEstudiante.Egresado, CondicionEstudiante.Desertor },
            [CondicionEstudiante.Egresado]     = new HashSet<CondicionEstudiante>(),   // estado terminal
            [CondicionEstudiante.Desertor]     = new HashSet<CondicionEstudiante> { CondicionEstudiante.Regular },
        };

    // ── Propiedades ──────────────────────────────────────────────────────────
    public int Id { get; private set; }
    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public int Anio { get; private set; }
    public CondicionEstudiante Condicion { get; private set; }
    public DateTime FechaDeIngreso { get; private set; }

    private Estudiante() { }

    // ── Factory ──────────────────────────────────────────────────────────────
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

    // ── Modificación de datos (sin tocar Condicion) ──────────────────────────
    public void Modificar(int anio)
    {
        if (anio < 1 || anio > 6) throw new ArgumentException("El año debe estar entre 1 y 6.");
        Anio = anio;
    }

    // ── Máquina de estados ───────────────────────────────────────────────────

    /// <summary>El estudiante pierde su regularidad (inasistencias / notas insuficientes).</summary>
    public void PerderRegularidad()
        => Transicionar(CondicionEstudiante.Libre, "Pérdida de regularidad");

    /// <summary>El estudiante obtiene condición promocional (cumple criterios del plan).</summary>
    public void ObtenerPromocion()
        => Transicionar(CondicionEstudiante.Promocional, "Obtención de condición promocional");

    /// <summary>El estudiante recupera la regularidad (ej.: re-cursado aprobado).</summary>
    public void RecuperarRegularidad()
        => Transicionar(CondicionEstudiante.Regular, "Recuperación de regularidad");

    /// <summary>El plan académico está 100% completo (CU-43: evento MateriaAprobada).</summary>
    public void Egresar()
        => Transicionar(CondicionEstudiante.Egresado, "Aprobación total del plan académico");

    /// <summary>Se detecta abandono (sin actividad en N períodos).</summary>
    public void Desertar()
        => Transicionar(CondicionEstudiante.Desertor, "Deserción detectada");

    /// <summary>El desertor se re-inscribe y vuelve al sistema como Regular.</summary>
    public void Reinscribir()
        => Transicionar(CondicionEstudiante.Regular, "Re-inscripción tras deserción");

    // ── Núcleo de la máquina de estados ─────────────────────────────────────
    private void Transicionar(CondicionEstudiante destino, string motivo)
    {
        if (Condicion == destino)
            return; // idempotente: ya está en ese estado

        if (!_transicionesValidas[Condicion].Contains(destino))
            throw new BusinessException(
                $"Transición inválida: {Condicion} → {destino}. Motivo solicitado: {motivo}.");

        var anterior = Condicion;
        Condicion = destino;

        AddEvent(new EstadoAcademicoChangedEvent(Id, anterior, destino, motivo));
    }
}
