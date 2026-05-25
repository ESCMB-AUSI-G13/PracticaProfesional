using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Tests.Domain;

public class ExamenTests
{
    private static DateTime FechaFutura() => DateTime.UtcNow.Date.AddDays(1);

    // ── Creación válida ───────────────────────────────────────────────────────

    [Fact]
    public void Crear_DatosValidos_RetornaExamen()
    {
        var examen = Examen.Crear(1, FechaFutura(), "09:00", 30, TipoExamen.Final);
        Assert.Equal(1, examen.MateriaId);
        Assert.Equal("09:00", examen.Horario);
        Assert.Equal(30, examen.Cupo);
        Assert.Equal(TipoExamen.Final, examen.TipoExamen);
    }

    [Theory]
    [InlineData(TipoExamen.Parcial)]
    [InlineData(TipoExamen.Final)]
    [InlineData(TipoExamen.Recuperatorio)]
    public void Crear_CualquierTipoExamen_AsignaTipoCorrectamente(TipoExamen tipo)
    {
        var examen = Examen.Crear(1, FechaFutura(), "09:00", 30, tipo);
        Assert.Equal(tipo, examen.TipoExamen);
    }

    // ── Validaciones de fecha ─────────────────────────────────────────────────

    [Fact]
    public void Crear_FechaEnPasado_LanzaArgumentException()
    {
        var ayer = DateTime.UtcNow.Date.AddDays(-1);
        Assert.Throws<ArgumentException>(() => Examen.Crear(1, ayer, "09:00", 30, TipoExamen.Final));
    }

    [Fact]
    public void Crear_FechaDeHoy_NoLanzaExcepcion()
    {
        var hoy = DateTime.UtcNow.Date;
        var examen = Examen.Crear(1, hoy, "09:00", 30, TipoExamen.Final);
        Assert.Equal(hoy, examen.FechaExamen);
    }

    // ── Validaciones de horario ───────────────────────────────────────────────

    [Fact]
    public void Crear_HorarioVacio_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Examen.Crear(1, FechaFutura(), "", 30, TipoExamen.Final));
    }

    [Fact]
    public void Crear_HorarioSoloEspacios_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Examen.Crear(1, FechaFutura(), "   ", 30, TipoExamen.Final));
    }

    // ── Validaciones de cupo ──────────────────────────────────────────────────

    [Fact]
    public void Crear_CupoCero_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Examen.Crear(1, FechaFutura(), "09:00", 0, TipoExamen.Final));
    }

    [Fact]
    public void Crear_CupoNegativo_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Examen.Crear(1, FechaFutura(), "09:00", -5, TipoExamen.Final));
    }

    // ── Modificar ─────────────────────────────────────────────────────────────

    [Fact]
    public void Modificar_DatosValidos_ActualizaPropiedades()
    {
        var examen = Examen.Crear(1, FechaFutura(), "09:00", 30, TipoExamen.Final);
        var nuevaFecha = FechaFutura().AddDays(5);
        examen.Modificar(nuevaFecha, "14:00", 50);
        Assert.Equal(nuevaFecha, examen.FechaExamen);
        Assert.Equal("14:00", examen.Horario);
        Assert.Equal(50, examen.Cupo);
    }

    [Fact]
    public void Modificar_HorarioVacio_LanzaArgumentException()
    {
        var examen = Examen.Crear(1, FechaFutura(), "09:00", 30, TipoExamen.Final);
        Assert.Throws<ArgumentException>(() => examen.Modificar(FechaFutura(), "", 30));
    }

    [Fact]
    public void Modificar_CupoNegativo_LanzaArgumentException()
    {
        var examen = Examen.Crear(1, FechaFutura(), "09:00", 30, TipoExamen.Final);
        Assert.Throws<ArgumentException>(() => examen.Modificar(FechaFutura(), "09:00", -1));
    }
}
