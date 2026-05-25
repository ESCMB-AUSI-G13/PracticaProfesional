using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Events;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Tests.Domain;

public class EstudianteTests
{
    private static Estudiante CrearRegular() =>
        Estudiante.Crear(1, 1, 1, DateTime.Today);

    // ── Creación ─────────────────────────────────────────────────────────────

    [Fact]
    public void Crear_EstadoInicialEsRegular()
    {
        var e = CrearRegular();
        Assert.Equal(CondicionEstudiante.Regular, e.Condicion);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void Crear_AnioFueraDeRango_LanzaArgumentException(int anio)
    {
        Assert.Throws<ArgumentException>(() => Estudiante.Crear(1, anio, 1, DateTime.Today));
    }

    [Fact]
    public void Crear_CarreraIdCero_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Estudiante.Crear(1, 1, 0, DateTime.Today));
    }

    // ── Transiciones válidas ──────────────────────────────────────────────────

    [Fact]
    public void PerderRegularidad_DesdeRegular_PasaALibre()
    {
        var e = CrearRegular();
        e.PerderRegularidad();
        Assert.Equal(CondicionEstudiante.Libre, e.Condicion);
    }

    [Fact]
    public void ObtenerPromocion_DesdeRegular_PasaAPromocional()
    {
        var e = CrearRegular();
        e.ObtenerPromocion();
        Assert.Equal(CondicionEstudiante.Promocional, e.Condicion);
    }

    [Fact]
    public void Egresar_DesdeRegular_PasaAEgresado()
    {
        var e = CrearRegular();
        e.Egresar();
        Assert.Equal(CondicionEstudiante.Egresado, e.Condicion);
    }

    [Fact]
    public void Desertar_DesdeRegular_PasaADesertor()
    {
        var e = CrearRegular();
        e.Desertar();
        Assert.Equal(CondicionEstudiante.Desertor, e.Condicion);
    }

    [Fact]
    public void RecuperarRegularidad_DesdeLibre_PasaARegular()
    {
        var e = CrearRegular();
        e.PerderRegularidad();
        e.RecuperarRegularidad();
        Assert.Equal(CondicionEstudiante.Regular, e.Condicion);
    }

    [Fact]
    public void Reinscribir_DesdeDesertor_PasaARegular()
    {
        var e = CrearRegular();
        e.Desertar();
        e.Reinscribir();
        Assert.Equal(CondicionEstudiante.Regular, e.Condicion);
    }

    // ── Estado terminal: Egresado ─────────────────────────────────────────────

    [Fact]
    public void Egresado_IntentarPerderRegularidad_LanzaBusinessException()
    {
        var e = CrearRegular();
        e.Egresar();
        Assert.Throws<BusinessException>(() => e.PerderRegularidad());
    }

    [Fact]
    public void Egresado_IntentarDesertar_LanzaBusinessException()
    {
        var e = CrearRegular();
        e.Egresar();
        Assert.Throws<BusinessException>(() => e.Desertar());
    }

    [Fact]
    public void Egresado_IntentarPromocion_LanzaBusinessException()
    {
        var e = CrearRegular();
        e.Egresar();
        Assert.Throws<BusinessException>(() => e.ObtenerPromocion());
    }

    // ── Idempotencia ──────────────────────────────────────────────────────────

    [Fact]
    public void PerderRegularidad_Repetido_EsIdempotente()
    {
        var e = CrearRegular();
        e.PerderRegularidad();
        e.PerderRegularidad(); // no debe lanzar excepción
        Assert.Equal(CondicionEstudiante.Libre, e.Condicion);
    }

    [Fact]
    public void Egresar_Repetido_EsIdempotente()
    {
        var e = CrearRegular();
        e.Egresar();
        e.Egresar(); // no debe lanzar excepción
        Assert.Equal(CondicionEstudiante.Egresado, e.Condicion);
    }

    // ── Domain Events ─────────────────────────────────────────────────────────

    [Fact]
    public void PerderRegularidad_EmiteDomainEvent()
    {
        var e = CrearRegular();
        e.PerderRegularidad();
        Assert.Single(e.DomainEvents);
        Assert.IsType<EstadoAcademicoChangedEvent>(e.DomainEvents[0]);
    }

    [Fact]
    public void TransicionIdempotente_NoEmiteDomainEvent()
    {
        var e = CrearRegular();
        e.PerderRegularidad();
        e.ClearEvents();
        e.PerderRegularidad(); // idempotente
        Assert.Empty(e.DomainEvents);
    }
}
