using PracticaProfesional.Application.Docentes.DTOs;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IDocenteRepository
{
    Task<Docente?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocenteDto>> ListarAsync(CancellationToken cancellationToken = default);
    Task AgregarAsync(Docente docente, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
