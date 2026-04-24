using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IMateriaRepository
{
    Task<IEnumerable<Materia>> ListarAsync(CancellationToken cancellationToken = default);
    Task<Materia?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistePorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<bool> ExistePorCodigoExcluyendoAsync(string codigo, int excludeId, CancellationToken cancellationToken = default);
    Task<int> ObtenerSiguienteNumeroAsync(CancellationToken cancellationToken = default);
    Task AgregarAsync(Materia materia, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
    Task<string?> ObtenerPlanAsync(int materiaId, CancellationToken cancellationToken = default);
    Task<int> ContarPorPlanAsync(string plan, CancellationToken cancellationToken = default);
}
