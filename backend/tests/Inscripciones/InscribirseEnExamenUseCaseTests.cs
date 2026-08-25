using PracticaProfesional.Application.Inscripciones;
using PracticaProfesional.Application.Inscripciones.DTOs;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;
using PracticaProfesional.Infrastructure.Persistence;
using PracticaProfesional.Infrastructure.Persistence.Repositories;
using PracticaProfesional.Tests.TestSupport;

namespace PracticaProfesional.Tests.Inscripciones;

/// <summary>
/// InscribirseEnExamenUseCase para un examen Final exige EstaRegularizadoAsync (no
/// "inscripción activa a la cursada"), respeta el cupo del examen y evita dobles
/// inscripciones. Estas tres reglas no tenían cobertura y son las que se corrigieron.
/// </summary>
public class InscribirseEnExamenUseCaseTests
{
    private sealed record Escenario(
        AppDbContext Db,
        InscribirseEnExamenUseCase UseCase,
        int UsuarioIdEstudiante,
        Estudiante Estudiante,
        Examen Examen,
        Materia Materia,
        Curso Curso);

    private static async Task<Escenario> PrepararEscenarioAsync(int cupoExamen = 30)
    {
        var db = InMemoryDb.Crear();

        var carrera = Carrera.Crear("Profesorado en Matemática", "Res. 001/2024");
        db.Carreras.Add(carrera);
        await db.SaveChangesAsync();

        var materia = Materia.Crear("MAT1", "Matemática I", carrera.Id, 1);
        db.Materias.Add(materia);
        await db.SaveChangesAsync();

        var usuarioPreceptor = Usuario.Crear("10000001", "PRE001", "preceptor3@test.com", "Ana", "Preceptora", "hash", Rol.Preceptor);
        db.Usuarios.Add(usuarioPreceptor);
        await db.SaveChangesAsync();
        var preceptor = Preceptor.Crear(usuarioPreceptor.Id, "3810000000", "Mañana");
        db.Preceptores.Add(preceptor);
        await db.SaveChangesAsync();

        var curso = Curso.Crear(2024, 1, "1A", cupo: 30, preceptor.Id, carrera.Id);
        db.Cursos.Add(curso);
        await db.SaveChangesAsync();

        var usuarioEstudiante = Usuario.Crear("20000001", "EST001", "alumno3@test.com", "Alumno", "Uno", "hash", Rol.Estudiante);
        db.Usuarios.Add(usuarioEstudiante);
        await db.SaveChangesAsync();
        var estudiante = Estudiante.Crear(usuarioEstudiante.Id, 1, carrera.Id, DateTime.UtcNow.AddYears(-1));
        db.Estudiantes.Add(estudiante);
        await db.SaveChangesAsync();

        var examen = Examen.CrearHistorico(materia.Id, DateTime.UtcNow.Date, "08:00", cupoExamen, TipoExamen.Final);
        db.Examenes.Add(examen);
        await db.SaveChangesAsync();

        // Período de inscripción a examen habilitado hoy (CU-47)
        db.CalendarioAcademico.Add(CalendarioAcademico.Crear(
            "Inscripción a finales", "", DateTime.UtcNow.Date.AddDays(-1), DateTime.UtcNow.Date.AddDays(1),
            TipoEvento.InscripcionExamen));
        await db.SaveChangesAsync();

        var useCase = new InscribirseEnExamenUseCase(
            new EstudianteRepository(db),
            new ExamenRepository(db),
            new InscripcionExamenRepository(db),
            new CorrelativiadadRepository(db),
            new HistorialAcademicoRepository(db),
            new CalendarioAcademicoRepository(db),
            new NoOpAuditoriaService(),
            new NoOpUnitOfWork());

        return new Escenario(db, useCase, usuarioEstudiante.Id, estudiante, examen, materia, curso);
    }

    private static async Task RegularizarAsync(Escenario e)
    {
        e.Db.HistorialAcademico.Add(HistorialAcademico.Crear(
            e.Estudiante.Id, e.Materia.Id, e.Curso.Id, e.Curso.Anio, e.Curso.Comision,
            estadoFinal: "Regular", notaFinal: null, CondicionEstudiante.Regular));
        await e.Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Inscribirse_SinCursadaRegularizada_Rechaza()
    {
        var e = await PrepararEscenarioAsync();

        var ex = await Assert.ThrowsAsync<BusinessException>(() =>
            e.UseCase.EjecutarAsync(new InscribirseEnExamenDto(e.Estudiante.Id, e.Examen.Id)));

        Assert.Contains("regularizada", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Inscribirse_ConCursadaRegularizada_Permite()
    {
        var e = await PrepararEscenarioAsync();
        await RegularizarAsync(e);

        var resultado = await e.UseCase.EjecutarAsync(new InscribirseEnExamenDto(e.Estudiante.Id, e.Examen.Id));

        Assert.Equal("Activa", resultado.Estado);
    }

    [Fact]
    public async Task Inscribirse_DosVecesAlMismoExamen_RechazaLaSegunda()
    {
        var e = await PrepararEscenarioAsync();
        await RegularizarAsync(e);
        await e.UseCase.EjecutarAsync(new InscribirseEnExamenDto(e.Estudiante.Id, e.Examen.Id));

        var ex = await Assert.ThrowsAsync<BusinessException>(() =>
            e.UseCase.EjecutarAsync(new InscribirseEnExamenDto(e.Estudiante.Id, e.Examen.Id)));

        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task Inscribirse_SinCupoDisponible_Rechaza()
    {
        // Cupo = 1, ya ocupado por otro alumno inscripto y regularizado.
        var e = await PrepararEscenarioAsync(cupoExamen: 1);
        await RegularizarAsync(e);

        var otroUsuario = Usuario.Crear("20000002", "EST002", "alumno4@test.com", "Alumno", "Dos", "hash", Rol.Estudiante);
        e.Db.Usuarios.Add(otroUsuario);
        await e.Db.SaveChangesAsync();
        var otroEstudiante = Estudiante.Crear(otroUsuario.Id, 1, e.Estudiante.CarreraId, DateTime.UtcNow.AddYears(-1));
        e.Db.Estudiantes.Add(otroEstudiante);
        await e.Db.SaveChangesAsync();
        e.Db.HistorialAcademico.Add(HistorialAcademico.Crear(
            otroEstudiante.Id, e.Materia.Id, e.Curso.Id, e.Curso.Anio, e.Curso.Comision,
            estadoFinal: "Regular", notaFinal: null, CondicionEstudiante.Regular));
        await e.Db.SaveChangesAsync();
        await e.UseCase.EjecutarAsync(new InscribirseEnExamenDto(otroEstudiante.Id, e.Examen.Id));

        var ex = await Assert.ThrowsAsync<BusinessException>(() =>
            e.UseCase.EjecutarAsync(new InscribirseEnExamenDto(e.Estudiante.Id, e.Examen.Id)));

        Assert.Equal(409, ex.StatusCode);
        Assert.Contains("cupo", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
