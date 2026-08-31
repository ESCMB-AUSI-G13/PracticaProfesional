using PracticaProfesional.Application.Cursos;
using PracticaProfesional.Application.Cursos.DTOs;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;
using PracticaProfesional.Infrastructure.Persistence.Repositories;
using PracticaProfesional.Tests.TestSupport;

namespace PracticaProfesional.Tests.Cursos;

/// <summary>
/// Regresión: se podía crear un curso asignándole un preceptor ya desactivado, porque
/// PreceptorRepository.ObtenerPorUsuarioIdAsync no filtra por Activo.
/// </summary>
public class CrearCursoUseCaseTests
{
    private static async Task<(
        Carrera carrera, Usuario usuarioPreceptor,
        CrearCursoUseCase useCase)>
        PrepararEscenarioAsync(bool preceptorActivo)
    {
        var db = InMemoryDb.Crear();

        var carrera = Carrera.Crear("Profesorado en Matemática", "Res. 001/2024");
        db.Carreras.Add(carrera);
        await db.SaveChangesAsync();

        var usuarioPreceptor = Usuario.Crear("10000001", "PRE001", "preceptor@test.com", "Ana", "Preceptora", "hash", Rol.Preceptor);
        if (!preceptorActivo) usuarioPreceptor.Desactivar();
        db.Usuarios.Add(usuarioPreceptor);
        await db.SaveChangesAsync();

        var preceptor = Preceptor.Crear(usuarioPreceptor.Id, "3810000000", "Mañana");
        db.Preceptores.Add(preceptor);
        await db.SaveChangesAsync();

        var useCase = new CrearCursoUseCase(
            new CursoRepository(db),
            new PreceptorRepository(db),
            new NoOpAuditoriaService());

        return (carrera, usuarioPreceptor, useCase);
    }

    [Fact]
    public async Task PreceptorActivo_CreaElCurso()
    {
        var (carrera, usuarioPreceptor, useCase) = await PrepararEscenarioAsync(preceptorActivo: true);

        var resultado = await useCase.EjecutarAsync(
            new CrearCursoDto(2026, 1, "1A", 30, usuarioPreceptor.Id, carrera.Id));

        Assert.NotNull(resultado);
    }

    [Fact]
    public async Task PreceptorInactivo_Rechaza409()
    {
        var (carrera, usuarioPreceptor, useCase) = await PrepararEscenarioAsync(preceptorActivo: false);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => useCase.EjecutarAsync(
            new CrearCursoDto(2026, 1, "1A", 30, usuarioPreceptor.Id, carrera.Id)));

        Assert.Equal(409, ex.StatusCode);
    }
}
