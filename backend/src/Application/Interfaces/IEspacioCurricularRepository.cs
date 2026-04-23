using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IEspacioCurricularRepository
{
    Task<IEnumerable<EspacioCurricular>> ListarAsync(CancellationToken cancellationToken = default);
    Task<EspacioCurricular?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(int materiaId, int docenteId, int cursoId, CancellationToken cancellationToken = default);
    Task AgregarAsync(EspacioCurricular ec, CancellationToken cancellationToken = default);
    Task EliminarAsync(EspacioCurricular ec, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
