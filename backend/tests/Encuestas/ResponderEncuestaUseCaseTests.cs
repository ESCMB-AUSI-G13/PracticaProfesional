using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PracticaProfesional.Application.Encuestas;
using PracticaProfesional.Application.Encuestas.DTOs;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;
using PracticaProfesional.Infrastructure.Persistence;
using PracticaProfesional.Infrastructure.Persistence.Repositories;
using PracticaProfesional.Tests.TestSupport;

namespace PracticaProfesional.Tests.Encuestas;

/// <summary>
/// La anonimización depende de que Encuestas:Salt esté configurado (sin default inseguro)
/// y de que el token de disociación bloquee una segunda respuesta del mismo alumno.
/// </summary>
public class ResponderEncuestaUseCaseTests
{
    private const string Salt = "test-salt-no-usar-en-produccion-1234567890";

    private sealed record Escenario(AppDbContext Db, Estudiante Estudiante, Encuesta Encuesta);

    private static async Task<Escenario> PrepararEscenarioAsync()
    {
        var db = InMemoryDb.Crear();

        var carrera = Carrera.Crear("Profesorado en Matemática", "Res. 001/2024");
        db.Carreras.Add(carrera);
        await db.SaveChangesAsync();

        var usuarioEstudiante = Usuario.Crear("20000001", "EST001", "alumno5@test.com", "Alumno", "Uno", "hash", Rol.Estudiante);
        db.Usuarios.Add(usuarioEstudiante);
        await db.SaveChangesAsync();
        var estudiante = Estudiante.Crear(usuarioEstudiante.Id, 1, carrera.Id, DateTime.UtcNow.AddYears(-1));
        db.Estudiantes.Add(estudiante);
        await db.SaveChangesAsync();

        var encuesta = Encuesta.Crear("Satisfacción general 2024", TipoEncuesta.SatisfaccionGeneral, 2024);
        db.Encuestas.Add(encuesta);
        await db.SaveChangesAsync();
        db.PreguntasEncuesta.Add(PreguntaEncuesta.Crear(encuesta.Id, "¿Cómo calificás la cursada?", 1, TipoPregunta.EscalaLikert));
        await db.SaveChangesAsync();

        return new Escenario(db, estudiante, encuesta);
    }

    private static ResponderEncuestaUseCase CrearUseCase(AppDbContext db, string? salt)
    {
        var configData = salt is null
            ? new Dictionary<string, string?>()
            : new Dictionary<string, string?> { ["Encuestas:Salt"] = salt };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

        return new ResponderEncuestaUseCase(new EncuestaRepository(db), new EstudianteRepository(db), config);
    }

    private static ResponderEncuestaDto CrearRespuesta(Escenario e)
    {
        var preguntaId = e.Encuesta.Preguntas.Single().Id;
        return new ResponderEncuestaDto(e.Encuesta.Id, [new ItemRespuestaDto(preguntaId, 5, null)]);
    }

    [Fact]
    public async Task Responder_PrimeraVez_Permite()
    {
        var e = await PrepararEscenarioAsync();
        var useCase = CrearUseCase(e.Db, Salt);

        await useCase.EjecutarAsync(e.Estudiante.UsuarioId, CrearRespuesta(e));

        Assert.Equal(1, await e.Db.RespuestasEncuesta.CountAsync());
    }

    [Fact]
    public async Task Responder_RespuestaAnonima_NoTieneFKAlEstudiante()
    {
        var e = await PrepararEscenarioAsync();
        var useCase = CrearUseCase(e.Db, Salt);

        await useCase.EjecutarAsync(e.Estudiante.UsuarioId, CrearRespuesta(e));

        // RespuestaEncuesta no expone ningún campo que referencie al estudiante — solo
        // EncuestaCompletada guarda un token, y ese token no es reversible sin el salt.
        var respuesta = await e.Db.RespuestasEncuesta.SingleAsync();
        var tipoRespuesta = typeof(RespuestaEncuesta);
        Assert.DoesNotContain(tipoRespuesta.GetProperties(), p => p.Name.Contains("Estudiante"));
        _ = respuesta;
    }

    [Fact]
    public async Task Responder_MismaEncuestaDosVeces_RechazaLaSegunda()
    {
        var e = await PrepararEscenarioAsync();
        var useCase = CrearUseCase(e.Db, Salt);
        await useCase.EjecutarAsync(e.Estudiante.UsuarioId, CrearRespuesta(e));

        var ex = await Assert.ThrowsAsync<BusinessException>(() =>
            useCase.EjecutarAsync(e.Estudiante.UsuarioId, CrearRespuesta(e)));

        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task Responder_SinSaltConfigurado_FallaExplicito_NoUsaDefaultInseguro()
    {
        var e = await PrepararEscenarioAsync();
        var useCase = CrearUseCase(e.Db, salt: null);

        // Sin Encuestas:Salt configurado no debe caer en un default público conocido:
        // tiene que fallar, no responder "silenciosamente" con un token reidentificable.
        await Assert.ThrowsAnyAsync<Exception>(() =>
            useCase.EjecutarAsync(e.Estudiante.UsuarioId, CrearRespuesta(e)));
    }
}
