using PracticaProfesional.Application.Cursos.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Cursos;

public class ListarCursosPorMateriaUseCase(
    ICursoRepository cursoRepository,
    IMateriaRepository materiaRepository,
    IEstudianteRepository estudianteRepository)
{
    /// <param name="esDireccion">true si el rol del solicitante es Direccion (sin restricción de
    /// carrera); si es false (Estudiante), solo puede listar cursos de materias de su propia
    /// carrera — evita ver cursos/preceptores de materias de otras carreras (IDOR).</param>
    public async Task<IEnumerable<CursoDto>> EjecutarAsync(
        int materiaId, int usuarioIdSolicitante, bool esDireccion, CancellationToken cancellationToken = default)
    {
        if (!esDireccion)
        {
            var estudiante = await estudianteRepository.ObtenerPorUsuarioIdAsync(usuarioIdSolicitante, cancellationToken)
                ?? throw new BusinessException($"No se encontró el perfil de estudiante para el usuario {usuarioIdSolicitante}.");

            var carreraIdMateria = await materiaRepository.ObtenerCarreraIdAsync(materiaId, cancellationToken);
            if (carreraIdMateria != estudiante.CarreraId)
                throw new BusinessException("No podés ver los cursos de una materia que no pertenece a tu carrera.", 403);
        }

        return await cursoRepository.ListarPorMateriaIdAsync(materiaId, cancellationToken);
    }
}
