using PracticaProfesional.Application.Reportes;

namespace PracticaProfesional.Tests.Reportes;

public class EgresadosPorCarreraUseCaseTests
{
    private static EgresadosPorCarreraUseCase Crear(RendimientoRepoFake fake)
        => new(fake);

    // ── Coherencia de totales ────────────────────────────────────────────────

    [Fact]
    public async Task TotalGeneral_EsIgualA_SumaDeFilas()
    {
        var fake = new RendimientoRepoFake
        {
            DatosEgresados =
            [
                ("Profesorado", 2021, 10, 60, 4.5),
                ("Profesorado", 2022,  8, 60, 3.8),
                ("Trayecto",    2021, 15, 50, 2.1),
            ]
        };

        var resultado = await Crear(fake).EjecutarAsync(null, null);

        Assert.Equal(33, resultado.TotalGeneral);
        Assert.Equal(33, resultado.Filas.Sum(f => f.TotalEgresados));
    }

    [Fact]
    public async Task TasaEgresoGlobal_EsConsistente_ConTotales()
    {
        var fake = new RendimientoRepoFake
        {
            DatosEgresados =
            [
                ("Profesorado", 2021, 20, 100, null),
            ]
        };

        var resultado = await Crear(fake).EjecutarAsync(null, null);

        // 20 / 100 * 100 = 20.0%
        Assert.Equal(20.0, resultado.TasaEgresoGlobal);
    }

    [Fact]
    public async Task TasaEgresoPorFila_EsConsistente_ConDatosDeEsaFila()
    {
        var fake = new RendimientoRepoFake
        {
            DatosEgresados =
            [
                ("Profesorado", 2021, 15, 60, null),
            ]
        };

        var resultado = await Crear(fake).EjecutarAsync(null, null);
        var fila = resultado.Filas.Single();

        // 15 / 60 * 100 = 25.0%
        Assert.Equal(25.0, fila.TasaEgreso);
    }

    // ── Duración promedio ponderada ──────────────────────────────────────────

    [Fact]
    public async Task DuracionPromedioGlobal_EsPonderada_PorCantidadDeEgresados()
    {
        // Cohorte A: 10 egresados con duración 2.0 años
        // Cohorte B:  2 egresados con duración 5.0 años
        // Promedio simple:    (2.0 + 5.0) / 2 = 3.5  ← incorrecto
        // Promedio ponderado: (10*2.0 + 2*5.0) / 12 = 30/12 = 2.5 ← correcto
        var fake = new RendimientoRepoFake
        {
            DatosEgresados =
            [
                ("Profesorado", 2021, 10, 60, 2.0),
                ("Profesorado", 2022,  2, 60, 5.0),
            ]
        };

        var resultado = await Crear(fake).EjecutarAsync(null, null);

        Assert.Equal(2.5, resultado.DuracionPromedioGlobal);
    }

    [Fact]
    public async Task DuracionPromedioGlobal_EsNull_SiNingunEgresadoTieneDuracion()
    {
        var fake = new RendimientoRepoFake
        {
            DatosEgresados =
            [
                ("Profesorado", 2025, 0, 60, null),
            ]
        };

        var resultado = await Crear(fake).EjecutarAsync(null, null);

        Assert.Null(resultado.DuracionPromedioGlobal);
    }

    [Fact]
    public async Task DuracionPromedioGlobal_IgnoraFilas_SinEgresados()
    {
        // Una fila con duración pero 0 egresados no debe contaminar el promedio
        var fake = new RendimientoRepoFake
        {
            DatosEgresados =
            [
                ("Profesorado", 2021, 10, 60, 3.0),
                ("Profesorado", 2025,  0, 60, 9.9), // 0 egresados → se ignora
            ]
        };

        var resultado = await Crear(fake).EjecutarAsync(null, null);

        Assert.Equal(3.0, resultado.DuracionPromedioGlobal);
    }

    // ── Casos borde ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SinDatos_DevuelveTotalesEnCero()
    {
        var fake = new RendimientoRepoFake();

        var resultado = await Crear(fake).EjecutarAsync(null, null);

        Assert.Equal(0, resultado.TotalGeneral);
        Assert.Equal(0.0, resultado.TasaEgresoGlobal);
        Assert.Empty(resultado.Filas);
    }

    [Fact]
    public async Task SinAlumnos_TasaEgresoEsCero_SinDivisionPorCero()
    {
        var fake = new RendimientoRepoFake
        {
            DatosEgresados = [("Profesorado", 2021, 0, 0, null)]
        };

        var resultado = await Crear(fake).EjecutarAsync(null, null);

        Assert.Equal(0.0, resultado.TasaEgresoGlobal);
    }
}
