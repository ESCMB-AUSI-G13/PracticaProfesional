using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IEstudianteRepository
{
    Task<Estudiante?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Estudiante>> ListarAsync(CancellationToken cancellationToken = default);
    Task AgregarAsync(Estudiante estudiante, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
