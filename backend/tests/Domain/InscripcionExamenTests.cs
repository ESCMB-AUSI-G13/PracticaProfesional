using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;
using PracticaProfesional.Domain.ValueObjects;

namespace PracticaProfesional.Tests.Domain;

public class InscripcionExamenTests
{
    // ── Creación ──────────────────────────────────────────────────────────────

    [Fact]
    public void Crear_EstadoInicialEsActiva()
    {
        var inscripcion = InscripcionExamen.Crear(1, 1);
        Assert.Equal(EstadoInscripcion.Activa, inscripcion.Estado);
    }

    [Fact]
    public void Crear_NotaValorInicialEsNull()
    {
        var inscripcion = InscripcionExamen.Crear(1, 1);
        Assert.Null(inscripcion.NotaValor);
    }

    // ── CargarNota ────────────────────────────────────────────────────────────

    [Fact]
    public void CargarNota_NotaAprobatoria_EstadoCambiaAAprobada()
    {
        var inscripcion = InscripcionExamen.Crear(1, 1);
        inscripcion.CargarNota(Nota.Crear(7));
        Assert.Equal(EstadoInscripcion.Aprobada, inscripcion.Estado);
    }

    [Fact]
    public void CargarNota_NotaDesaprobatoria_EstadoCambiaADesaprobada()
    {
        var inscripcion = InscripcionExamen.Crear(1, 1);
        inscripcion.CargarNota(Nota.Crear(3));
        Assert.Equal(EstadoInscripcion.Desaprobada, inscripcion.Estado);
    }

    [Fact]
    public void CargarNota_NotaLimiteAprobado_EstadoAprobada()
    {
        var inscripcion = InscripcionExamen.Crear(1, 1);
        inscripcion.CargarNota(Nota.Crear(4));
        Assert.Equal(EstadoInscripcion.Aprobada, inscripcion.Estado);
    }

    [Fact]
    public void CargarNota_PersisiteValorEnNotaValor()
    {
        var inscripcion = InscripcionExamen.Crear(1, 1);
        inscripcion.CargarNota(Nota.Crear(8));
        Assert.Equal(8m, inscripcion.NotaValor);
    }

    // ── RectificarNota ────────────────────────────────────────────────────────

    [Fact]
    public void RectificarNota_EstadoActiva_LanzaBusinessException()
    {
        var inscripcion = InscripcionExamen.Crear(1, 1);
        Assert.Throws<BusinessException>(() => inscripcion.RectificarNota(Nota.Crear(5)));
    }

    [Fact]
    public void RectificarNota_EstadoBaja_LanzaBusinessException()
    {
        var inscripcion = InscripcionExamen.Crear(1, 1);
        inscripcion.DarDeBaja();
        Assert.Throws<BusinessException>(() => inscripcion.RectificarNota(Nota.Crear(5)));
    }

    [Fact]
    public void RectificarNota_DesdeAprobada_ActualizaNota()
    {
        var inscripcion = InscripcionExamen.Crear(1, 1);
        inscripcion.CargarNota(Nota.Crear(6));
        inscripcion.RectificarNota(Nota.Crear(8));
        Assert.Equal(8m, inscripcion.NotaValor);
        Assert.Equal(EstadoInscripcion.Aprobada, inscripcion.Estado);
    }

    [Fact]
    public void RectificarNota_DesdeDesaprobada_ActualizaNota()
    {
        var inscripcion = InscripcionExamen.Crear(1, 1);
        inscripcion.CargarNota(Nota.Crear(2));
        inscripcion.RectificarNota(Nota.Crear(5));
        Assert.Equal(5m, inscripcion.NotaValor);
        Assert.Equal(EstadoInscripcion.Aprobada, inscripcion.Estado);
    }

    [Fact]
    public void RectificarNota_AprobadaARectificadaDesaprobatoria_CambiaEstado()
    {
        var inscripcion = InscripcionExamen.Crear(1, 1);
        inscripcion.CargarNota(Nota.Crear(7));
        inscripcion.RectificarNota(Nota.Crear(2));
        Assert.Equal(EstadoInscripcion.Desaprobada, inscripcion.Estado);
    }

    // ── DarDeBaja ─────────────────────────────────────────────────────────────

    [Fact]
    public void DarDeBaja_CambiaEstadoABaja()
    {
        var inscripcion = InscripcionExamen.Crear(1, 1);
        inscripcion.DarDeBaja();
        Assert.Equal(EstadoInscripcion.Baja, inscripcion.Estado);
    }
}
