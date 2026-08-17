using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Calificaciones;
using PracticaProfesional.Application.Calificaciones.DTOs;
using PracticaProfesional.Application.EstadoAcademico;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;
using PracticaProfesional.Infrastructure.Persistence;
using PracticaProfesional.Infrastructure.Persistence.Repositories;
using PracticaProfesional.Tests.TestSupport;

namespace PracticaProfesional.Tests.Calificaciones;

/// <summary>
/// Verifica que aprobar el examen Final de una materia (a) quede como nota definitiva
/// de la materia en HistorialAcademico y (b) dispare egreso automático cuando esa era la
/// última materia pendiente del plan (CU-43, evento "MateriaAprobada").
/// </summary>
public class CargarNotaExamenUseCaseTests
{
    private sealed record Escenario(
        AppDbContext Db,
        CargarNotaExamenUseCase UseCase,
        Estudiante Estudiante,
        Materia Materia,
        Curso Curso,
        InscripcionExamen InscripcionExamen,
        int UsuarioIdDocente,
        int UsuarioIdOtroDocente);

    private static async Task<Escenario> PrepararEscenarioAsync()
    {
        var db = InMemoryDb.Crear();

        var carrera = Carrera.Crear("Profesorado en Matemática", "Res. 001/2024");
        db.Carreras.Add(carrera);
        await db.SaveChangesAsync();

        // Carrera con una sola materia: aprobarla completa el 100% del plan.
        var materia = Materia.Crear("MAT1", "Matemática I", carrera.Id, 1);
        db.Materias.Add(materia);
        await db.SaveChangesAsync();

        var usuarioPreceptor = Usuario.Crear("10000001", "PRE001", "preceptor2@test.com", "Ana", "Preceptora", "hash", Rol.Preceptor);
        db.Usuarios.Add(usuarioPreceptor);
        await db.SaveChangesAsync();
        var preceptor = Preceptor.Crear(usuarioPreceptor.Id, "3810000000", "Mañana");
        db.Preceptores.Add(preceptor);
        await db.SaveChangesAsync();

        var curso = Curso.Crear(2024, 1, "1A", cupo: 30, preceptor.Id, carrera.Id);
        db.Cursos.Add(curso);
        await db.SaveChangesAsync();

        var usuarioDocente = Usuario.Crear("10000002", "DOC001", "docente2@test.com", "Juan", "Docente", "hash", Rol.Docente);
        var usuarioOtroDocente = Usuario.Crear("10000003", "DOC002", "otrodocente@test.com", "Pedro", "Otro", "hash", Rol.Docente);
        db.Usuarios.AddRange(usuarioDocente, usuarioOtroDocente);
        await db.SaveChangesAsync();
        var docente = Docente.Crear(usuarioDocente.Id, "3810000001", "Titular");
        var otroDocente = Docente.Crear(usuarioOtroDocente.Id, "3810000002", "Titular");
        db.Docentes.AddRange(docente, otroDocente);
        await db.SaveChangesAsync();

        // Solo "docente" dicta MAT1; "otroDocente" no tiene ningún espacio curricular.
        db.EspaciosCurriculares.Add(EspacioCurricular.Crear(materia.Id, docente.Id, curso.Id));
        await db.SaveChangesAsync();

        var usuarioEstudiante = Usuario.Crear("20000001", "EST001", "alumno2@test.com", "Alumno", "Uno", "hash", Rol.Estudiante);
        db.Usuarios.Add(usuarioEstudiante);
        await db.SaveChangesAsync();
        var estudiante = Estudiante.Crear(usuarioEstudiante.Id, 1, carrera.Id, DateTime.UtcNow.AddYears(-1));
        db.Estudiantes.Add(estudiante);
        await db.SaveChangesAsync();

        // Simula que la cursada ya se cerró como Regular (lo haría CerrarCursoUseCase).
        db.HistorialAcademico.Add(HistorialAcademico.Crear(
            estudiante.Id, materia.Id, curso.Id, curso.Anio, curso.Comision,
            estadoFinal: "Regular", notaFinal: null, CondicionEstudiante.Regular));
        await db.SaveChangesAsync();

        var examen = Examen.CrearHistorico(materia.Id, DateTime.UtcNow.Date, "08:00", cupo: 30, TipoExamen.Final);
        db.Examenes.Add(examen);
        await db.SaveChangesAsync();

        var inscripcionExamen = InscripcionExamen.Crear(estudiante.Id, examen.Id);
        db.InscripcionesExamen.Add(inscripcionExamen);
        await db.SaveChangesAsync();

        var useCase = new CargarNotaExamenUseCase(
            new InscripcionExamenRepository(db),
            new HistorialAcademicoRepository(db),
            new DocenteRepository(db),
            new EspacioCurricularRepository(db),
            new ActualizarEstadoAcademicoUseCase(
                new EstudianteRepository(db),
                new HistorialAcademicoRepository(db),
                new AsistenciaRepository(db),
                new MateriaRepository(db),
                new InscripcionMateriaRepository(db),
                new NoOpAuditoriaService()),
            new NoOpAuditoriaService());

        return new Escenario(db, useCase, estudiante, materia, curso, inscripcionExamen, usuarioDocente.Id, usuarioOtroDocente.Id);
    }

    [Fact]
    public async Task CargarNota_FinalAprobado_ActualizaNotaFinalEnHistorial()
    {
        var e = await PrepararEscenarioAsync();

        await e.UseCase.EjecutarAsync(new CargarNotaExamenDto(e.InscripcionExamen.Id, 8m, e.UsuarioIdDocente));

        var historial = await e.Db.HistorialAcademico.AsNoTracking()
            .SingleAsync(h => h.EstudianteId == e.Estudiante.Id && h.MateriaId == e.Materia.Id);
        Assert.Equal(8m, historial.NotaFinal);
    }

    [Fact]
    public async Task CargarNota_UltimaMateriaDelPlanAprobada_DisparaEgresoAutomatico()
    {
        var e = await PrepararEscenarioAsync();

        await e.UseCase.EjecutarAsync(new CargarNotaExamenDto(e.InscripcionExamen.Id, 7m, e.UsuarioIdDocente));

        var estudianteActualizado = await e.Db.Estudiantes.AsNoTracking().SingleAsync(x => x.Id == e.Estudiante.Id);
        Assert.Equal(CondicionEstudiante.Egresado, estudianteActualizado.Condicion);
    }

    [Fact]
    public async Task CargarNota_FinalDesaprobado_NoTocaElHistorialNiEgresa()
    {
        var e = await PrepararEscenarioAsync();

        await e.UseCase.EjecutarAsync(new CargarNotaExamenDto(e.InscripcionExamen.Id, 3m, e.UsuarioIdDocente));

        var historial = await e.Db.HistorialAcademico.AsNoTracking()
            .SingleAsync(h => h.EstudianteId == e.Estudiante.Id && h.MateriaId == e.Materia.Id);
        var estudianteActualizado = await e.Db.Estudiantes.AsNoTracking().SingleAsync(x => x.Id == e.Estudiante.Id);

        Assert.Null(historial.NotaFinal);
        Assert.Equal(CondicionEstudiante.Regular, estudianteActualizado.Condicion);
    }

    [Fact]
    public async Task CargarNota_DocenteQueNoDictaLaMateria_RechazaConForbidden()
    {
        var e = await PrepararEscenarioAsync();

        var ex = await Assert.ThrowsAsync<BusinessException>(() =>
            e.UseCase.EjecutarAsync(new CargarNotaExamenDto(e.InscripcionExamen.Id, 8m, e.UsuarioIdOtroDocente)));

        Assert.Equal(403, ex.StatusCode);
    }
}
