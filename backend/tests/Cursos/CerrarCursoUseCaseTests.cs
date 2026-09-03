using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Cursos;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Infrastructure.Persistence.Repositories;
using PracticaProfesional.Tests.TestSupport;

namespace PracticaProfesional.Tests.Cursos;

/// <summary>
/// CerrarCursoUseCase es el punto donde HistorialAcademico empieza a existir con datos
/// reales (antes solo lo poblaban los seeders). Estos tests verifican que la liquidación
/// de la cursada (Regular/Libre según asistencia) quede escrita correctamente.
/// </summary>
public class CerrarCursoUseCaseTests
{
    /// <summary>Usuario Dirección ficticio: los tests de liquidación cierran como Dirección.</summary>
    private const int UsuarioDireccionId = 999;

    private static async Task<(
        Curso curso, Materia materia, Estudiante regular, Estudiante libre,
        CerrarCursoUseCase useCase,
        PracticaProfesional.Infrastructure.Persistence.AppDbContext db)>
        PrepararEscenarioAsync()
    {
        var db = InMemoryDb.Crear();

        var carrera = Carrera.Crear("Profesorado en Matemática", "Res. 001/2024");
        db.Carreras.Add(carrera);
        await db.SaveChangesAsync();

        var materia = Materia.Crear("MAT1", "Matemática I", carrera.Id, 1);
        db.Materias.Add(materia);
        await db.SaveChangesAsync();

        var usuarioPreceptor = Usuario.Crear("10000001", "PRE001", "preceptor@test.com", "Ana", "Preceptora", "hash", Rol.Preceptor);
        db.Usuarios.Add(usuarioPreceptor);
        await db.SaveChangesAsync();
        var preceptor = Preceptor.Crear(usuarioPreceptor.Id, "3810000000", "Mañana");
        db.Preceptores.Add(preceptor);
        await db.SaveChangesAsync();

        var usuarioDocente = Usuario.Crear("10000002", "DOC001", "docente@test.com", "Juan", "Docente", "hash", Rol.Docente);
        db.Usuarios.Add(usuarioDocente);
        await db.SaveChangesAsync();
        var docente = Docente.Crear(usuarioDocente.Id, "3810000001", "Titular");
        db.Docentes.Add(docente);
        await db.SaveChangesAsync();

        var curso = Curso.Crear(2024, 1, "1A", cupo: 30, preceptor.Id, carrera.Id);
        db.Cursos.Add(curso);
        await db.SaveChangesAsync();

        db.EspaciosCurriculares.Add(EspacioCurricular.Crear(materia.Id, docente.Id, curso.Id));
        await db.SaveChangesAsync();

        var usuarioRegular = Usuario.Crear("20000001", "EST001", "regular@test.com", "Alumno", "Regular", "hash", Rol.Estudiante);
        var usuarioLibre = Usuario.Crear("20000002", "EST002", "libre@test.com", "Alumno", "Libre", "hash", Rol.Estudiante);
        db.Usuarios.AddRange(usuarioRegular, usuarioLibre);
        await db.SaveChangesAsync();

        var estudianteRegular = Estudiante.Crear(usuarioRegular.Id, 1, carrera.Id, DateTime.UtcNow.AddYears(-1));
        var estudianteLibre = Estudiante.Crear(usuarioLibre.Id, 1, carrera.Id, DateTime.UtcNow.AddYears(-1));
        db.Estudiantes.AddRange(estudianteRegular, estudianteLibre);
        await db.SaveChangesAsync();

        db.InscripcionesMateria.Add(InscripcionMateria.Crear(estudianteRegular.Id, materia.Id, curso.Id));
        db.InscripcionesMateria.Add(InscripcionMateria.Crear(estudianteLibre.Id, materia.Id, curso.Id));
        await db.SaveChangesAsync();

        // Estudiante "regular": 10 clases, 1 ausente injustificado (10 % — bajo el umbral del 25 %)
        for (var i = 0; i < 9; i++)
            db.Asistencias.Add(Asistencia.Registrar(estudianteRegular.Id, materia.Id, curso.Id, DateTime.UtcNow.AddDays(-i), EstadoAsistencia.Presente));
        db.Asistencias.Add(Asistencia.Registrar(estudianteRegular.Id, materia.Id, curso.Id, DateTime.UtcNow.AddDays(-9), EstadoAsistencia.Ausente));

        // Estudiante "libre": 10 clases, 3 ausentes injustificados (30 % — supera el umbral del 25 %)
        for (var i = 0; i < 7; i++)
            db.Asistencias.Add(Asistencia.Registrar(estudianteLibre.Id, materia.Id, curso.Id, DateTime.UtcNow.AddDays(-i), EstadoAsistencia.Presente));
        for (var i = 7; i < 10; i++)
            db.Asistencias.Add(Asistencia.Registrar(estudianteLibre.Id, materia.Id, curso.Id, DateTime.UtcNow.AddDays(-i), EstadoAsistencia.Ausente));
        await db.SaveChangesAsync();

        var useCase = new CerrarCursoUseCase(
            new CursoRepository(db),
            new InscripcionMateriaRepository(db),
            new AsistenciaRepository(db),
            new HistorialAcademicoRepository(db),
            new PreceptorRepository(db),
            new NoOpAuditoriaService(),
            new NoOpUnitOfWork());

        return (curso, materia, estudianteRegular, estudianteLibre, useCase, db);
    }

    [Fact]
    public async Task Ejecutar_CierraElCurso()
    {
        var (curso, _, _, _, useCase, db) = await PrepararEscenarioAsync();

        await useCase.EjecutarAsync(curso.Id, UsuarioDireccionId, esDireccion: true);

        var cursoActualizado = await db.Cursos.AsNoTracking().SingleAsync(c => c.Id == curso.Id);
        Assert.Equal(EstadoCurso.Cerrado, cursoActualizado.Estado);
    }

    [Fact]
    public async Task Ejecutar_AlumnoConBajaAusencia_QuedaRegularEnHistorial()
    {
        var (curso, materia, estudianteRegular, _, useCase, db) = await PrepararEscenarioAsync();

        await useCase.EjecutarAsync(curso.Id, UsuarioDireccionId, esDireccion: true);

        var historial = await db.HistorialAcademico.AsNoTracking()
            .SingleAsync(h => h.EstudianteId == estudianteRegular.Id && h.MateriaId == materia.Id);

        Assert.Equal(CondicionEstudiante.Regular, historial.Condicion);
        Assert.Null(historial.NotaFinal);
        Assert.Equal(curso.Id, historial.CursoId);
    }

    [Fact]
    public async Task Ejecutar_AlumnoConAltaAusencia_QuedaLibreEnHistorial()
    {
        var (curso, materia, _, estudianteLibre, useCase, db) = await PrepararEscenarioAsync();

        await useCase.EjecutarAsync(curso.Id, UsuarioDireccionId, esDireccion: true);

        var historial = await db.HistorialAcademico.AsNoTracking()
            .SingleAsync(h => h.EstudianteId == estudianteLibre.Id && h.MateriaId == materia.Id);

        Assert.Equal(CondicionEstudiante.Libre, historial.Condicion);
    }

    [Fact]
    public async Task Ejecutar_AlumnoRegular_QuedaRegularizadoParaCorrelatividades()
    {
        var (curso, materia, estudianteRegular, _, useCase, db) = await PrepararEscenarioAsync();
        await useCase.EjecutarAsync(curso.Id, UsuarioDireccionId, esDireccion: true);

        var historialRepo = new HistorialAcademicoRepository(db);
        var regularizado = await historialRepo.EstaRegularizadoAsync(estudianteRegular.Id, materia.Id);

        Assert.True(regularizado);
    }

    [Fact]
    public async Task Ejecutar_AlumnoLibre_NoQuedaRegularizadoParaCorrelatividades()
    {
        var (curso, materia, _, estudianteLibre, useCase, db) = await PrepararEscenarioAsync();
        await useCase.EjecutarAsync(curso.Id, UsuarioDireccionId, esDireccion: true);

        var historialRepo = new HistorialAcademicoRepository(db);
        var regularizado = await historialRepo.EstaRegularizadoAsync(estudianteLibre.Id, materia.Id);

        Assert.False(regularizado);
    }

    [Fact]
    public async Task Ejecutar_MarcaLaInscripcionMateriaSegunElResultado()
    {
        var (curso, materia, estudianteRegular, estudianteLibre, useCase, db) = await PrepararEscenarioAsync();

        await useCase.EjecutarAsync(curso.Id, UsuarioDireccionId, esDireccion: true);

        var inscRegular = await db.InscripcionesMateria.AsNoTracking()
            .SingleAsync(i => i.EstudianteId == estudianteRegular.Id && i.MateriaId == materia.Id);
        var inscLibre = await db.InscripcionesMateria.AsNoTracking()
            .SingleAsync(i => i.EstudianteId == estudianteLibre.Id && i.MateriaId == materia.Id);

        Assert.Equal(EstadoInscripcion.Aprobada, inscRegular.Estado);
        Assert.Equal(EstadoInscripcion.Desaprobada, inscLibre.Estado);
    }

    // ── Propiedad del curso (CU-33) ──────────────────────────────────────────────
    // El endpoint aceptaba el rol Preceptor pero no validaba de quién era el curso, así que
    // cualquier preceptor podía liquidar la cursada de un curso ajeno.

    private static async Task<(
        Curso cursoDeAna, Usuario usuarioAna, Usuario usuarioBeto, CerrarCursoUseCase useCase)>
        PrepararDosPreceptoresAsync()
    {
        var db = InMemoryDb.Crear();

        var carrera = Carrera.Crear("Profesorado en Matemática", "Res. 001/2024");
        db.Carreras.Add(carrera);
        await db.SaveChangesAsync();

        var usuarioAna = Usuario.Crear("10000001", "PRE001", "ana@test.com", "Ana", "Preceptora", "hash", Rol.Preceptor);
        var usuarioBeto = Usuario.Crear("10000002", "PRE002", "beto@test.com", "Beto", "Preceptor", "hash", Rol.Preceptor);
        db.Usuarios.AddRange(usuarioAna, usuarioBeto);
        await db.SaveChangesAsync();

        var ana = Preceptor.Crear(usuarioAna.Id, "3810000000", "Mañana");
        var beto = Preceptor.Crear(usuarioBeto.Id, "3810000001", "Tarde");
        db.Preceptores.AddRange(ana, beto);
        await db.SaveChangesAsync();

        var cursoDeAna = Curso.Crear(2026, 1, "1A", cupo: 30, ana.Id, carrera.Id);
        db.Cursos.Add(cursoDeAna);
        await db.SaveChangesAsync();

        var useCase = new CerrarCursoUseCase(
            new CursoRepository(db),
            new InscripcionMateriaRepository(db),
            new AsistenciaRepository(db),
            new HistorialAcademicoRepository(db),
            new PreceptorRepository(db),
            new NoOpAuditoriaService(),
            new NoOpUnitOfWork());

        return (cursoDeAna, usuarioAna, usuarioBeto, useCase);
    }

    [Fact]
    public async Task Preceptor_CierraSuPropioCurso_Permite()
    {
        var (cursoDeAna, usuarioAna, _, useCase) = await PrepararDosPreceptoresAsync();

        await useCase.EjecutarAsync(cursoDeAna.Id, usuarioAna.Id, esDireccion: false);

        Assert.Equal(EstadoCurso.Cerrado, cursoDeAna.Estado);
    }

    [Fact]
    public async Task Preceptor_IntentaCerrarCursoAjeno_Rechaza403()
    {
        var (cursoDeAna, _, usuarioBeto, useCase) = await PrepararDosPreceptoresAsync();

        var ex = await Assert.ThrowsAsync<PracticaProfesional.Domain.Exceptions.BusinessException>(
            () => useCase.EjecutarAsync(cursoDeAna.Id, usuarioBeto.Id, esDireccion: false));

        Assert.Equal(403, ex.StatusCode);
        Assert.Equal(EstadoCurso.Activo, cursoDeAna.Estado);
    }
}
