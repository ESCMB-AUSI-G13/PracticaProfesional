using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IAuditoriaRepository
{
    Task RegistrarAsync(AuditoriaCambioRol registro, CancellationToken cancellationToken = default);
}
