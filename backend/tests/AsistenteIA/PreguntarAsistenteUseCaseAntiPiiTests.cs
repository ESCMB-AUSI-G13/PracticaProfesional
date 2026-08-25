using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.AI;
using PracticaProfesional.Application.AsistenteIA;
using PracticaProfesional.Application.Encuestas;
using PracticaProfesional.Application.Reportes;
using PracticaProfesional.Application.Reportes.DTOs;
using PracticaProfesional.Application.Carreras;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence.Repositories;
using PracticaProfesional.Tests.Reportes;
using PracticaProfesional.Tests.TestSupport;

namespace PracticaProfesional.Tests.AsistenteIA;

/// <summary>
/// Test de caracterización (safety net) para la regla anti-PII del Asistente de IA: ninguna
/// herramienta expuesta a Gemini vía tool-calling debe reenviar nombre/legajo/DNI de un alumno.
/// Antes de este test, esa garantía dependía 100% de que un desarrollador recordara envolver
/// una herramienta nueva en una función Proyectar* — sin ningún control automático que rompiera
/// el build si no lo hacía (ver CHECKLIST.md, Tier 6 #29).
///
/// Se siembran datos con marcadores de PII fácilmente identificables (Legajo/Nombre/Apellido
/// con el sufijo "PIIMARKER") y se invoca cada herramienta devuelta por ConstruirHerramientas()
/// tal como lo haría el SDK de tool-calling — si algún resultado serializado contiene esos
/// marcadores, el test falla.
/// </summary>
public class PreguntarAsistenteUseCaseAntiPiiTests
{
    private const string LegajoPii    = "PIIMARKER-LEGAJO-9001";
    private const string NombrePii    = "PIIMARKERNombre";
    private const string ApellidoPii  = "PIIMARKERApellido";

    private static async Task<List<AITool>> ConstruirHerramientasDePruebaAsync()
    {
        var db = InMemoryDb.Crear();

        var carrera = Carrera.Crear("Carrera Test AntiPII", "Res. 000/2026");
        db.Carreras.Add(carrera);
        await db.SaveChangesAsync();

        var materia = Materia.Crear("APII1", "Materia AntiPII", carrera.Id, 1);
        db.Materias.Add(materia);
        await db.SaveChangesAsync();

        var usuarioPreceptor = Usuario.Crear("30000001", "PREAPII", "preceptor.apii@test.com", "Preceptor", "AntiPii", "hash", Rol.Preceptor);
        db.Usuarios.Add(usuarioPreceptor);
        await db.SaveChangesAsync();
        var preceptor = Preceptor.Crear(usuarioPreceptor.Id, "3810000000", "Mañana");
        db.Preceptores.Add(preceptor);
        await db.SaveChangesAsync();

        var curso = Curso.Crear(2026, 1, "APII", cupo: 30, preceptor.Id, carrera.Id);
        db.Cursos.Add(curso);
        await db.SaveChangesAsync();

        // Estudiante con datos de PII marcados — es el que NO debería aparecer en ningún
        // resultado serializado de las herramientas.
        var usuarioEstudiante = Usuario.Crear("30000002", LegajoPii, "alumno.apii@test.com", NombrePii, ApellidoPii, "hash", Rol.Estudiante);
        db.Usuarios.Add(usuarioEstudiante);
        await db.SaveChangesAsync();
        var estudiante = Estudiante.Crear(usuarioEstudiante.Id, 1, carrera.Id, DateTime.UtcNow.AddYears(-1));
        db.Estudiantes.Add(estudiante);
        await db.SaveChangesAsync();

        // Una ausencia real, para que ReporteInasistenciasUseCase tenga algo que proyectar.
        db.Asistencias.Add(Asistencia.Registrar(estudiante.Id, materia.Id, curso.Id, DateTime.UtcNow.Date, EstadoAsistencia.Ausente));
        await db.SaveChangesAsync();

        // IRendimientoConsolidadoRepository: fake configurable ya usado en otros tests de
        // Reportes — se le carga un estudiante en riesgo con los mismos marcadores de PII.
        var rendimientoFake = new RendimientoRepoFake
        {
            DatosRiesgo =
            [
                new DatosRiesgoEstudianteDto
                {
                    EstudianteId = estudiante.Id,
                    Legajo       = LegajoPii,
                    Nombre       = NombrePii,
                    Apellido     = ApellidoPii,
                    Carrera      = carrera.Nombre,
                    AnioCarrera  = 1,
                    AnioCohorte  = DateTime.UtcNow.Year,
                    Condicion    = "Libre", // fuerza NivelRiesgo = "Alto"
                    TotalClases  = 10,
                    Ausencias    = 6
                }
            ]
        };

        var preguntarUseCase = new PreguntarAsistenteUseCase(
            asistenteIA: null!, // no se usa: se invoca ConstruirHerramientas directamente
            auditoria: new NoOpAuditoriaService(),
            listarCarrerasUseCase: new ListarCarrerasUseCase(new CarreraRepository(db)),
            tableroUseCase: new TableroEjecutivoUseCase(rendimientoFake),
            riesgoUseCase: new RiesgoAcademicoUseCase(rendimientoFake),
            retencionCohorteUseCase: new RetencionPorCohorteUseCase(rendimientoFake),
            retencionAnualUseCase: new RetencionAnualUseCase(rendimientoFake),
            desercionUseCase: new DesercionPorAnioUseCase(rendimientoFake),
            egresadosUseCase: new EgresadosPorCarreraUseCase(rendimientoFake),
            comparativoUseCase: new ComparativoComisionesUseCase(rendimientoFake),
            promediosUseCase: new PromediosCatedraUseCase(rendimientoFake),
            evolucionUseCase: new EvolucionNotasUseCase(rendimientoFake),
            inasistenciasUseCase: new ReporteInasistenciasUseCase(new AsistenciaRepository(db)),
            encuestasUseCase: new ResultadosEncuestasUseCase(new EncuestaRepository(db)));

        // ConstruirHerramientas es private — se invoca por reflection, tal como lo haría el
        // SDK de tool-calling en producción (a través de PreguntarAsync → ChatOptions.Tools).
        var metodo = typeof(PreguntarAsistenteUseCase).GetMethod(
            "ConstruirHerramientas", BindingFlags.NonPublic | BindingFlags.Instance)!;

        return (List<AITool>)metodo.Invoke(preguntarUseCase, [CancellationToken.None])!;
    }

    [Fact]
    public async Task NingunaHerramienta_ExponeListadoNominalDeAlumnos()
    {
        var herramientas = await ConstruirHerramientasDePruebaAsync();

        // Confirma que el catálogo no se achicó silenciosamente (si alguien borra una
        // herramienta, este test debe notarlo y actualizarse a propósito, no pasar solo).
        Assert.Equal(11, herramientas.Count);

        var fallas = new List<string>();

        foreach (var herramienta in herramientas)
        {
            var funcion = Assert.IsAssignableFrom<AIFunction>(herramienta);

            // AIFunctionFactory marca como "requerido" todo parámetro sin default explícito en
            // C# (aunque sea Nullable<T>) — hay que pasar cada uno en null a propósito, tal
            // como lo haría el modelo si decide omitir un filtro opcional. Se arma de forma
            // genérica a partir del JSON Schema de la propia herramienta, sin listar a mano
            // los parámetros de cada una.
            var argumentos = new AIFunctionArguments();
            if (funcion.JsonSchema.TryGetProperty("properties", out var propiedades))
            {
                foreach (var prop in propiedades.EnumerateObject())
                    argumentos[prop.Name] = null;
            }

            object? resultado;
            try
            {
                resultado = await funcion.InvokeAsync(argumentos, cancellationToken: CancellationToken.None);
            }
            catch (Exception ex)
            {
                fallas.Add($"{funcion.Name}: la herramienta lanzó una excepción al invocarla sin argumentos ({ex.GetType().Name}: {ex.Message})");
                continue;
            }

            var json = JsonSerializer.Serialize(resultado);

            if (json.Contains(LegajoPii) || json.Contains(NombrePii) || json.Contains(ApellidoPii))
                fallas.Add($"{funcion.Name}: el resultado serializado contiene datos de PII del alumno de prueba — JSON: {json}");
        }

        Assert.True(fallas.Count == 0, "Herramientas que filtran PII:\n" + string.Join("\n", fallas));
    }
}
