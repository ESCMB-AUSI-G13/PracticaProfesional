using PracticaProfesional.Application.Cursos;
using PracticaProfesional.Application.Cursos.DTOs;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;
using PracticaProfesional.Infrastructure.Persistence.Repositories;
using PracticaProfesional.Tests.TestSupport;

namespace PracticaProfesional.Tests.Cursos;

/// <summary>
/// Regresión: reasignar el preceptor de un curso a otro que está desactivado no rechazaba nada.
/// Mantener el preceptor YA asignado al curso sigue permitido aunque se haya desactivado después
/// (editar-curso.component.ts lo deja seleccionado a propósito para no bloquear ediciones de
/// cupo/comisión).
/// </summary>
public class ModificarCursoUseCaseTests
{
    private static async Task<(
        Curso curso, Usuario usuarioPreceptorOriginal, Usuario usuarioPreceptorOtro,
        ModificarCursoUseCase useCase)>
        PrepararEscenarioAsync()
    {
        var db = InMemoryDb.Crear();

        var carrera = Carrera.Crear("Profesorado en Matemática", "Res. 001/2024");
        db.Carreras.Add(carrera);
        await db.SaveChangesAsync();

        var usuarioPreceptorOriginal = Usuario.Crear("10000001", "PRE001", "original@test.com", "Ana", "Original", "hash", Rol.Preceptor);
        var usuarioPreceptorOtro = Usuario.Crear("10000002", "PRE002", "otro@test.com", "Beto", "Otro", "hash", Rol.Preceptor);
        db.Usuarios.AddRange(usuarioPreceptorOriginal, usuarioPreceptorOtro);
        await db.SaveChangesAsync();

        var preceptorOriginal = Preceptor.Crear(usuarioPreceptorOriginal.Id, "3810000000", "Mañana");
        var preceptorOtro = Preceptor.Crear(usuarioPreceptorOtro.Id, "3810000001", "Tarde");
        db.Preceptores.AddRange(preceptorOriginal, preceptorOtro);
        await db.SaveChangesAsync();

        var curso = Curso.Crear(2026, 1, "1A", cupo: 30, preceptorOriginal.Id, carrera.Id);
        db.Cursos.Add(curso);
        await db.SaveChangesAsync();

        // Ambos preceptores se desactivan DESPUÉS de que el curso ya los tenía disponibles,
        // para simular el escenario real: la desactivación es un evento posterior a la carga.
        usuarioPreceptorOriginal.Desactivar();
        usuarioPreceptorOtro.Desactivar();
        await db.SaveChangesAsync();

        var useCase = new ModificarCursoUseCase(
            new CursoRepository(db),
            new PreceptorRepository(db),
            new NoOpAuditoriaService());

        return (curso, usuarioPreceptorOriginal, usuarioPreceptorOtro, useCase);
    }

    [Fact]
    public async Task MantenerElPreceptorYaAsignado_PermiteAunqueEsteInactivo()
    {
        var (curso, usuarioPreceptorOriginal, _, useCase) = await PrepararEscenarioAsync();

        var resultado = await useCase.EjecutarAsync(
            curso.Id, new ModificarCursoDto("1B", 25, usuarioPreceptorOriginal.Id));

        Assert.Equal("1B", resultado.Comision);
    }

    [Fact]
    public async Task ReasignarAOtroPreceptorInactivo_Rechaza409()
    {
        var (curso, _, usuarioPreceptorOtro, useCase) = await PrepararEscenarioAsync();

        var ex = await Assert.ThrowsAsync<BusinessException>(() => useCase.EjecutarAsync(
            curso.Id, new ModificarCursoDto("1B", 25, usuarioPreceptorOtro.Id)));

        Assert.Equal(409, ex.StatusCode);
    }
}
