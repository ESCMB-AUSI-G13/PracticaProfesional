using PracticaProfesional.Application.Usuarios;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;
using PracticaProfesional.Infrastructure.Persistence.Repositories;
using PracticaProfesional.Tests.TestSupport;

namespace PracticaProfesional.Tests.Usuarios;

/// <summary>
/// Regresión de la validación manual (Tarea 4 del Acta de Pruebas): un usuario Dirección lograba
/// desactivar su propia cuenta cuando existía otra cuenta Dirección activa, cortando su propia
/// sesión con un error confuso en el frontend. No hay caso de uso real para autodesactivarse, así
/// que ahora se bloquea siempre, sin importar cuántas otras cuentas Dirección sigan activas.
/// </summary>
public class CambiarActivacionUseCaseTests
{
    private static async Task<(
        Usuario direccionA, Usuario direccionB,
        CambiarActivacionUseCase useCase)>
        PrepararEscenarioAsync()
    {
        var db = InMemoryDb.Crear();

        var direccionA = Usuario.Crear("30000001", "DIR001", "direcciona@test.com", "Ana", "Directora", "hash", Rol.Direccion);
        var direccionB = Usuario.Crear("30000002", "DIR002", "direccionb@test.com", "Beto", "Director", "hash", Rol.Direccion);
        db.Usuarios.AddRange(direccionA, direccionB);
        await db.SaveChangesAsync();

        var useCase = new CambiarActivacionUseCase(
            new UsuarioRepository(db),
            new NoOpAuditoriaService());

        return (direccionA, direccionB, useCase);
    }

    [Fact]
    public async Task Direccion_DesactivaOtraCuentaDireccion_ConOtraActivaDeSobra_Permite()
    {
        var (direccionA, direccionB, useCase) = await PrepararEscenarioAsync();

        await useCase.EjecutarAsync(
            direccionB.Id, activar: false, Rol.Direccion, "Usuario",
            usuarioIdSolicitante: direccionA.Id);

        Assert.True(true); // no lanzó excepción
    }

    [Fact]
    public async Task Direccion_IntentaAutodesactivarse_RechazaAunqueHayaOtraCuentaActiva()
    {
        var (direccionA, _, useCase) = await PrepararEscenarioAsync();

        var ex = await Assert.ThrowsAsync<BusinessException>(() => useCase.EjecutarAsync(
            direccionA.Id, activar: false, Rol.Direccion, "Usuario",
            usuarioIdSolicitante: direccionA.Id));

        Assert.Equal(409, ex.StatusCode);
    }
}
