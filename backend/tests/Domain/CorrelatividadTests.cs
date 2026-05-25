using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Tests.Domain;

public class CorrelatividadTests
{
    [Fact]
    public void Crear_MateriasDistintas_RetornaCorrelatividad()
    {
        var c = Correlatividad.Crear(1, 2, "Cursar", CondicionAcademica.Regularizado);
        Assert.Equal(1, c.MateriaDestinoId);
        Assert.Equal(2, c.MateriaRequisitoId);
        Assert.Equal("Cursar", c.TipoRequerimiento);
    }

    [Fact]
    public void Crear_MismaMateria_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Correlatividad.Crear(1, 1, "Cursar", CondicionAcademica.Regularizado));
    }

    [Fact]
    public void Crear_TipoRequerimientoVacio_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Correlatividad.Crear(1, 2, "", CondicionAcademica.Regularizado));
    }

    [Fact]
    public void Crear_TipoRequerimientoSoloEspacios_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Correlatividad.Crear(1, 2, "   ", CondicionAcademica.Regularizado));
    }

    [Theory]
    [InlineData(CondicionAcademica.Regularizado)]
    [InlineData(CondicionAcademica.Aprobado)]
    public void Crear_CondicionAcademica_AsignaCorrectamente(CondicionAcademica condicion)
    {
        var c = Correlatividad.Crear(1, 2, "Cursar", condicion);
        Assert.Equal(condicion, c.CondicionAcademica);
    }

    [Fact]
    public void Crear_CondicionRegularizado_RequiereRegularidad()
    {
        var c = Correlatividad.Crear(1, 2, "Cursar", CondicionAcademica.Regularizado);
        Assert.Equal(CondicionAcademica.Regularizado, c.CondicionAcademica);
    }

    [Fact]
    public void Crear_CondicionAprobado_RequiereAprobacion()
    {
        var c = Correlatividad.Crear(1, 2, "Rendir", CondicionAcademica.Aprobado);
        Assert.Equal(CondicionAcademica.Aprobado, c.CondicionAcademica);
    }
}
