using PracticaProfesional.Application.Reportes;
using PracticaProfesional.Application.Reportes.DTOs;

namespace PracticaProfesional.Tests.Reportes;

public class TableroEjecutivoUseCaseTests
{
    private static TableroEjecutivoUseCase Crear(RendimientoRepoFake fake)
        => new(fake);

    // ── Coherencia de matrícula ──────────────────────────────────────────────

    [Fact]
    public async Task TotalHistorico_EsIgualA_MatriculadosMasEgresadosMasDesertores()
    {
        var fake = new RendimientoRepoFake
        {
            DatosCohorte =
            [
                new DatosCohorteDto { AnioCohorte = 2021, Carrera = "Profesorado", Total = 60, Activos = 30, Egresados = 20, Desertores = 10 },
                new DatosCohorteDto { AnioCohorte = 2022, Carrera = "Profesorado", Total = 50, Activos = 35, Egresados = 10, Desertores = 5  },
            ]
        };

        var resultado = await Crear(fake).EjecutarAsync();

        Assert.Equal(110, resultado.TotalHistorico);
        Assert.Equal(resultado.TotalHistorico,
            resultado.TotalMatriculados + resultado.TotalEgresados + resultado.TotalDesertores);
    }

    [Fact]
    public async Task TasaDesercionGlobal_EsConsistente_ConTotales()
    {
        var fake = new RendimientoRepoFake
        {
            DatosCohorte =
            [
                new DatosCohorteDto { AnioCohorte = 2021, Carrera = "Profesorado", Total = 100, Activos = 70, Egresados = 20, Desertores = 10 },
            ]
        };

        var resultado = await Crear(fake).EjecutarAsync();

        // 10 / 100 * 100 = 10.0%
        Assert.Equal(10.0m, resultado.TasaDesercionGlobal);
    }

    [Fact]
    public async Task TasaEgresoGlobal_EsConsistente_ConTotales()
    {
        var fake = new RendimientoRepoFake
        {
            DatosCohorte =
            [
                new DatosCohorteDto { AnioCohorte = 2021, Carrera = "Profesorado", Total = 100, Activos = 70, Egresados = 25, Desertores = 5 },
            ]
        };

        var resultado = await Crear(fake).EjecutarAsync();

        // 25 / 100 * 100 = 25.0%
        Assert.Equal(25.0m, resultado.TasaEgresoGlobal);
    }

    [Fact]
    public async Task TasaRetencion_Incluye_MatriculadosYEgresados()
    {
        var fake = new RendimientoRepoFake
        {
            DatosCohorte =
            [
                new DatosCohorteDto { AnioCohorte = 2021, Carrera = "Profesorado", Total = 100, Activos = 60, Egresados = 20, Desertores = 20 },
            ]
        };

        var resultado = await Crear(fake).EjecutarAsync();

        // Retención = (activos + egresados) / total = (60 + 20) / 100 = 80.0%
        Assert.Equal(80.0m, resultado.TasaRetencionGlobal);
    }

    // ── Coherencia de condición académica ───────────────────────────────────

    [Fact]
    public async Task Condiciones_SumanExactamente_TotalDeActivos()
    {
        var fake = new RendimientoRepoFake
        {
            DatosRiesgo =
            [
                new() { EstudianteId = 1, Condicion = "Promocional" },
                new() { EstudianteId = 2, Condicion = "Promocional" },
                new() { EstudianteId = 3, Condicion = "Regular"     },
                new() { EstudianteId = 4, Condicion = "Regular"     },
                new() { EstudianteId = 5, Condicion = "Regular"     },
                new() { EstudianteId = 6, Condicion = "Libre"       },
            ]
        };

        var resultado = await Crear(fake).EjecutarAsync();

        Assert.Equal(2, resultado.Promocionales);
        Assert.Equal(3, resultado.Regulares);
        Assert.Equal(1, resultado.Libres);
        Assert.Equal(6, resultado.Promocionales + resultado.Regulares + resultado.Libres);
    }

    // ── Rendimiento global ponderado ─────────────────────────────────────────

    [Fact]
    public async Task PromedioNotaGlobal_EsPonderado_PorCantidadDeAlumnos()
    {
        // Cátedra A: promedio 8.0 con 10 alumnos
        // Cátedra B: promedio 4.0 con 10 alumnos
        // Promedio simple:    (8.0 + 4.0) / 2 = 6.0
        // Promedio ponderado: (8.0*10 + 4.0*10) / 20 = 6.0 (coincide por pesos iguales)
        // Cátedra C: promedio 9.0 con 20 alumnos → ponderado = (80 + 40 + 180) / 40 = 7.5
        var fake = new RendimientoRepoFake
        {
            DatosCatedras =
            [
                new(1, "Mat A", "Docente", "A", 2021, 10, 10, 8, 2, 8.0m, 80m),
                new(2, "Mat B", "Docente", "A", 2021, 10, 10, 5, 5, 4.0m, 50m),
                new(3, "Mat C", "Docente", "A", 2021, 20, 20, 18, 2, 9.0m, 90m),
            ]
        };

        var resultado = await Crear(fake).EjecutarAsync();

        // (8.0*10 + 4.0*10 + 9.0*20) / 40 = (80 + 40 + 180) / 40 = 300 / 40 = 7.5
        Assert.Equal(7.50m, resultado.PromedioNotaGlobal);
    }

    [Fact]
    public async Task PorcentajeAprobacion_EsConsistente_ConAprobadosYTotalConNota()
    {
        var fake = new RendimientoRepoFake
        {
            DatosCatedras =
            [
                new(1, "Mat A", "Docente", "A", 2021, 10, 10, 7, 3, 6.0m, 70m),
                new(2, "Mat B", "Docente", "A", 2021, 10, 10, 3, 7, 4.0m, 30m),
            ]
        };

        var resultado = await Crear(fake).EjecutarAsync();

        // (7 + 3) aprobados / (10 + 10) con nota = 10/20 = 50%
        Assert.Equal(50.0m, resultado.PorcentajeAprobacionGlobal);
    }

    // ── Casos borde ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SinDatos_DevuelveTasasEnCero_SinDivisionPorCero()
    {
        var fake = new RendimientoRepoFake();

        var resultado = await Crear(fake).EjecutarAsync();

        Assert.Equal(0m, resultado.TasaDesercionGlobal);
        Assert.Equal(0m, resultado.TasaEgresoGlobal);
        Assert.Equal(0m, resultado.TasaRetencionGlobal);
        Assert.Equal(0m, resultado.PorcentajeAprobacionGlobal);
        Assert.Null(resultado.PromedioNotaGlobal);
    }

    [Fact]
    public async Task EvolucionCohortes_AgrupaPorAnio_SumandoCorrectamente()
    {
        var fake = new RendimientoRepoFake
        {
            DatosCohorte =
            [
                new DatosCohorteDto { AnioCohorte = 2021, Carrera = "Profesorado", Total = 60, Activos = 40, Egresados = 15, Desertores = 5 },
                new DatosCohorteDto { AnioCohorte = 2021, Carrera = "Trayecto",    Total = 40, Activos = 25, Egresados = 10, Desertores = 5 },
            ]
        };

        var resultado = await Crear(fake).EjecutarAsync();

        var cohorte2021 = resultado.EvolucionCohortes.Single(e => e.AnioCohorte == 2021);
        Assert.Equal(100, cohorte2021.Total);
        Assert.Equal(65,  cohorte2021.Activos);
        Assert.Equal(25,  cohorte2021.Egresados);
        Assert.Equal(10,  cohorte2021.Desertores);
    }
}
