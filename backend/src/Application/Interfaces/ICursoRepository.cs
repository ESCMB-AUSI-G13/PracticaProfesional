using PracticaProfesional.Application.Cursos.DTOs;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface ICursoRepository
{
    Task<IEnumerable<CursoDto>> ListarAsync(CancellationToken cancellationToken = default);
    Task<Curso?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistePorAnioYComisionAsync(int anio, int anioLectivo, string comision, int carreraId, CancellationToken cancellationToken = default);
    Task<bool> ExistePorAnioYComisionExcluyendoAsync(int anio, int anioLectivo, string comision, int carreraId, int excludeId, CancellationToken cancellationToken = default);
    Task AgregarAsync(Curso curso, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
