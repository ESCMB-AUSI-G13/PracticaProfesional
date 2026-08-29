using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Application.Cursos;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;
using PracticaProfesional.Infrastructure.Persistence.Repositories;
using PracticaProfesional.Tests.TestSupport;

namespace PracticaProfesional.Tests.Cursos;

/// <summary>
/// Regresión para el IDOR donde un Estudiante podía pasar cualquier materiaId y ver cursos
/// (comisión, cupo, preceptor) de materias de otra carrera a la que no pertenece.
/// </summary>
public class ListarCursosPorMateriaUseCaseTests
{
    private static async Task<(
        Materia materiaCarreraA, Materia materiaCarreraB,
        Usuario usuarioEstudianteA,
        ListarCursosPorMateriaUseCase useCase)>
        PrepararEscenarioAsync()
    {
        var db = InMemoryDb.Crear();

        var carreraA = Carrera.Crear("Profesorado en Matemática", "Res. 001/2024");
        var carreraB = Carrera.Crear("Profesorado en Historia", "Res. 002/2024");
        db.Carreras.AddRange(carreraA, carreraB);
        await db.SaveChangesAsync();

        var materiaCarreraA = Materia.Crear("MAT1", "Matemática I", carreraA.Id, 1);
        var materiaCarreraB = Materia.Crear("HIS1", "Historia I", carreraB.Id, 1);
        db.Materias.AddRange(materiaCarreraA, materiaCarreraB);
        await db.SaveChangesAsync();

        var usuarioEstudianteA = Usuario.Crear("20000001", "EST001", "estudiantea@test.com", "Alumno", "CarreraA", "hash", Rol.Estudiante);
        db.Usuarios.Add(usuarioEstudianteA);
        await db.SaveChangesAsync();

        var estudianteA = Estudiante.Crear(usuarioEstudianteA.Id, 1, carreraA.Id, DateTime.UtcNow.AddYears(-1));
        db.Estudiantes.Add(estudianteA);
        await db.SaveChangesAsync();

        var useCase = new ListarCursosPorMateriaUseCase(
            new CursoRepository(db),
            new MateriaRepository(db),
            new EstudianteRepository(db));

        return (materiaCarreraA, materiaCarreraB, usuarioEstudianteA, useCase);
    }

    [Fact]
    public async Task Estudiante_PidiendoMateriaDeSuPropiaCarrera_ListaOk()
    {
        var (materiaCarreraA, _, usuarioEstudianteA, useCase) = await PrepararEscenarioAsync();

        var resultado = await useCase.EjecutarAsync(materiaCarreraA.Id, usuarioEstudianteA.Id, esDireccion: false);

        Assert.NotNull(resultado);
    }

    [Fact]
    public async Task Estudiante_PidiendoMateriaDeOtraCarrera_Rechaza403()
    {
        var (_, materiaCarreraB, usuarioEstudianteA, useCase) = await PrepararEscenarioAsync();

        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => useCase.EjecutarAsync(materiaCarreraB.Id, usuarioEstudianteA.Id, esDireccion: false));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task Direccion_PidiendoCualquierMateria_NoValidaCarrera()
    {
        var (_, materiaCarreraB, usuarioEstudianteA, useCase) = await PrepararEscenarioAsync();

        var resultado = await useCase.EjecutarAsync(materiaCarreraB.Id, usuarioEstudianteA.Id, esDireccion: true);

        Assert.NotNull(resultado);
    }
}
