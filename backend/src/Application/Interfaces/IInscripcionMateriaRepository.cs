using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IInscripcionMateriaRepository
{
    Task<bool> ExisteInscripcionActivaAsync(int estudianteId, int materiaId, CancellationToken cancellationToken = default);
    Task<InscripcionMateria?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task AgregarAsync(InscripcionMateria inscripcion, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
