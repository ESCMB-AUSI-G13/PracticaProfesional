using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IInscripcionExamenRepository
{
    /// <summary>Retorna una inscripción a examen por su Id, incluyendo Estudiante, Usuario y Examen/Materia.</summary>
    Task<InscripcionExamen?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Retorna todas las inscripciones activas de un examen, ordenadas por apellido.</summary>
    Task<IEnumerable<InscripcionExamen>> ObtenerPorExamenAsync(int examenId, CancellationToken cancellationToken = default);

    /// <summary>Persiste los cambios tracked por EF Core (notas cargadas, estados actualizados).</summary>
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
