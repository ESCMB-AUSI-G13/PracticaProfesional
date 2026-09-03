using PracticaProfesional.Application.Cursos.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Cursos;

/// <summary>
/// CU-33: lista los cursos a cargo del preceptor autenticado, para que pueda cerrar sus actas.
/// El endpoint general de cursos es de Dirección y devuelve todos los del instituto; este solo
/// devuelve los propios.
/// </summary>
public class ListarCursosPreceptorUseCase(
    IPreceptorRepository preceptorRepository,
    ICursoRepository cursoRepository)
{
    public async Task<IEnumerable<CursoDto>> EjecutarAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var preceptor = await preceptorRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el perfil de preceptor para el usuario {usuarioId}.");

        return await cursoRepository.ListarPorPreceptorIdAsync(preceptor.Id, cancellationToken);
    }
}
