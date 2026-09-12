using PracticaProfesional.Application.Cursos.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Cursos;

/// <summary>
/// Dirección ve todos los cursos del instituto. Un Estudiante solo puede ver los de su propia
/// carrera — antes este listado no filtraba nada para Estudiante y devolvía todos los cursos de
/// todas las carreras (mismo tipo de fuga que ListarCursosPorMateriaUseCase ya corregía).
/// </summary>
public class ListarCursosUseCase(ICursoRepository cursoRepository, IEstudianteRepository estudianteRepository)
{
    public async Task<IEnumerable<CursoDto>> EjecutarAsync(
        int usuarioIdSolicitante, bool esDireccion, CancellationToken cancellationToken = default)
    {
        var cursos = await cursoRepository.ListarAsync(cancellationToken);
        if (esDireccion) return cursos;

        var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioIdSolicitante, cancellationToken)
            ?? throw new BusinessException($"No se encontró el perfil de estudiante para el usuario {usuarioIdSolicitante}.");

        return cursos.Where(c => c.CarreraId == estudiante.CarreraId);
    }
}
