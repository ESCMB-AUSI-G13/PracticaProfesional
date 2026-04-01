using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Persistence.Repositories;

public class AuditoriaRepository(AppDbContext context) : IAuditoriaRepository
{
    public async Task RegistrarAsync(AuditoriaCambioRol registro, CancellationToken cancellationToken = default)
    {
        await context.AuditoriaCambiosRol.AddAsync(registro, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
