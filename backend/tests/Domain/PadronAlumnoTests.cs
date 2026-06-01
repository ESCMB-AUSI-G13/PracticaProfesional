using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Tests.Domain;

public class PadronAlumnoTests
{
    [Theory]
    [InlineData("12345678")]
    [InlineData("1234567")]
    [InlineData("1234567890")]
    public void Crear_DniValido_RetornaPadronAlumno(string dni)
    {
        var padron = PadronAlumno.Crear(dni);
        Assert.Equal(dni, padron.DNI);
    }

    [Fact]
    public void Crear_DniConEspacios_SeHaceTrim()
    {
        var padron = PadronAlumno.Crear("  12345678  ");
        Assert.Equal("12345678", padron.DNI);
    }

    [Fact]
    public void Crear_DniVacio_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => PadronAlumno.Crear(""));
    }

    [Fact]
    public void Crear_DniSoloEspacios_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => PadronAlumno.Crear("   "));
    }

    [Theory]
    [InlineData("1234567A")]
    [InlineData("abc12345")]
    [InlineData("12-34567")]
    public void Crear_DniConLetrasOCaracteresEspeciales_LanzaArgumentException(string dni)
    {
        Assert.Throws<ArgumentException>(() => PadronAlumno.Crear(dni));
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("12345")]
    public void Crear_DniMenorA7Digitos_LanzaArgumentException(string dni)
    {
        Assert.Throws<ArgumentException>(() => PadronAlumno.Crear(dni));
    }

    [Fact]
    public void Crear_DniMayorA10Digitos_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => PadronAlumno.Crear("12345678901"));
    }

    [Fact]
    public void Crear_FechaCargaEsAproximadamenteAhora()
    {
        var antes = DateTime.UtcNow.AddSeconds(-1);
        var padron = PadronAlumno.Crear("12345678");
        var despues = DateTime.UtcNow.AddSeconds(1);
        Assert.InRange(padron.FechaCarga, antes, despues);
    }
}
