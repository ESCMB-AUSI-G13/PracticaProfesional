using PracticaProfesional.Application.Cursos.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Cursos;

/// <summary>
/// Un Estudiante solo puede consultar cursos de materias de su propia carrera — sin este chequeo,
/// pedir el endpoint directo por URL con el Id de una materia de otra carrera devolvía sus cursos
/// igual (nombre de preceptor incluido), aunque el desplegable del frontend nunca lo ofreciera.
/// </summary>
public class ListarCursosPorMateriaUseCase(
    ICursoRepository cursoRepository,
    IMateriaRepository materiaRepository,
    IEstudianteRepository estudianteRepository)
{
    public async Task<IEnumerable<CursoDto>> EjecutarAsync(
        int materiaId, int usuarioId, bool esDireccion, CancellationToken cancellationToken = default)
    {
        if (!esDireccion)
        {
            var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
                ?? throw new BusinessException($"No se encontró el perfil de estudiante para el usuario {usuarioId}.");

            var carreraIdMateria = await materiaRepository.ObtenerCarreraIdAsync(materiaId, cancellationToken);
            if (carreraIdMateria is null || carreraIdMateria != estudiante.CarreraId)
                throw new BusinessException("No tenés permiso para ver los cursos de esta materia.", 403);
        }

        return await cursoRepository.ListarPorMateriaIdAsync(materiaId, cancellationToken);
    }
}
