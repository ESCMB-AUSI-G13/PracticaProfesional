using PracticaProfesional.Domain.Exceptions;
using PracticaProfesional.Domain.ValueObjects;

namespace PracticaProfesional.Tests.Domain;

public class NotaTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Crear_ValorValido_RetornaNota(decimal valor)
    {
        var nota = Nota.Crear(valor);
        Assert.Equal(valor, nota.Valor);
    }

    [Fact]
    public void Crear_ValorMenorA1_LanzaBusinessException()
    {
        Assert.Throws<BusinessException>(() => Nota.Crear(0.9m));
    }

    [Fact]
    public void Crear_ValorCero_LanzaBusinessException()
    {
        Assert.Throws<BusinessException>(() => Nota.Crear(0));
    }

    [Fact]
    public void Crear_ValorMayorA10_LanzaBusinessException()
    {
        Assert.Throws<BusinessException>(() => Nota.Crear(10.1m));
    }

    [Fact]
    public void Crear_ValorNegativo_LanzaBusinessException()
    {
        Assert.Throws<BusinessException>(() => Nota.Crear(-1));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(10)]
    public void EsAprobado_NotaIgualOmayorA4_RetornaTrue(decimal valor)
    {
        var nota = Nota.Crear(valor);
        Assert.True(nota.EsAprobado);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(3.99)]
    public void EsAprobado_NotaMenorA4_RetornaFalse(decimal valor)
    {
        var nota = Nota.Crear(valor);
        Assert.False(nota.EsAprobado);
    }

    [Fact]
    public void Crear_RedondeoA2Decimales()
    {
        var nota = Nota.Crear(7.555m);
        Assert.Equal(Math.Round(7.555m, 2), nota.Valor);
    }

    [Fact]
    public void Igualdad_MismoValor_SonIguales()
    {
        var a = Nota.Crear(7);
        var b = Nota.Crear(7);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Igualdad_ValoresDiferentes_NoSonIguales()
    {
        var a = Nota.Crear(5);
        var b = Nota.Crear(8);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ToString_FormatoConDosDecimales()
    {
        var nota = Nota.Crear(7);
        Assert.Equal(7m.ToString("F2"), nota.ToString());
    }
}
