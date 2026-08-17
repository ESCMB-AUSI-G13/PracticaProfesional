using Microsoft.EntityFrameworkCore;
using PracticaProfesional.Infrastructure.Persistence;

namespace PracticaProfesional.Tests.TestSupport;

/// <summary>
/// Crea un AppDbContext respaldado por el proveedor InMemory de EF Core, aislado por test.
/// Permite ejercitar los UseCases contra los repositorios reales (sin fakes por interfaz)
/// sembrando entidades de dominio directamente.
/// </summary>
internal static class InMemoryDb
{
    public static AppDbContext Crear()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
