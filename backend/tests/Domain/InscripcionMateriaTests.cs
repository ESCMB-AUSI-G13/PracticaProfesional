using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Tests.Domain;

public class InscripcionMateriaTests
{
    [Fact]
    public void Crear_EstadoInicialEsActiva()
    {
        var inscripcion = InscripcionMateria.Crear(1, 1, 1);
        Assert.Equal(EstadoInscripcion.Activa, inscripcion.Estado);
    }

    [Fact]
    public void Crear_AsignaIdsCorrectos()
    {
        var inscripcion = InscripcionMateria.Crear(5, 10, 15);
        Assert.Equal(5, inscripcion.EstudianteId);
        Assert.Equal(10, inscripcion.MateriaId);
        Assert.Equal(15, inscripcion.CursoId);
    }

    [Fact]
    public void Crear_FechaInscripcionEsAproximadamenteAhora()
    {
        var antes = DateTime.UtcNow.AddSeconds(-1);
        var inscripcion = InscripcionMateria.Crear(1, 1, 1);
        var despues = DateTime.UtcNow.AddSeconds(1);
        Assert.InRange(inscripcion.FechaInscripcion, antes, despues);
    }

    [Fact]
    public void DarDeBaja_CambiaEstadoABaja()
    {
        var inscripcion = InscripcionMateria.Crear(1, 1, 1);
        inscripcion.DarDeBaja();
        Assert.Equal(EstadoInscripcion.Baja, inscripcion.Estado);
    }

    [Fact]
    public void MarcarAprobada_CambiaEstadoAAprobada()
    {
        var inscripcion = InscripcionMateria.Crear(1, 1, 1);
        inscripcion.MarcarAprobada();
        Assert.Equal(EstadoInscripcion.Aprobada, inscripcion.Estado);
    }

    [Fact]
    public void MarcarDesaprobada_CambiaEstadoADesaprobada()
    {
        var inscripcion = InscripcionMateria.Crear(1, 1, 1);
        inscripcion.MarcarDesaprobada();
        Assert.Equal(EstadoInscripcion.Desaprobada, inscripcion.Estado);
    }
}
