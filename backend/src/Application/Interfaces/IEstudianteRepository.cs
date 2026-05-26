using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IEstudianteRepository
{
    Task<Estudiante?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Estudiante?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default);

    /// <summary>RR-09: Busca un estudiante por el legajo de su usuario.</summary>
    Task<Estudiante?> ObtenerPorLegajoAsync(string legajo, CancellationToken cancellationToken = default);
    Task<IEnumerable<Estudiante>> ListarAsync(CancellationToken cancellationToken = default);
    Task AgregarAsync(Estudiante estudiante, CancellationToken cancellationToken = default);
    Task EliminarAsync(int estudianteId, int usuarioId, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
