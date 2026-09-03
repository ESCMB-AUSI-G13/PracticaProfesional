using PracticaProfesional.Application.EspaciosCurriculares.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.EspaciosCurriculares;

/// <summary>
/// Cátedras sobre las que el preceptor autenticado puede trabajar: las dictadas en los cursos
/// que tiene a cargo. El equivalente del docente es ListarEspaciosDocenteUseCase; el preceptor
/// no tenía ninguno, así que la pantalla de Rectificar Asistencias caía al endpoint de listado
/// general (Dirección-only) y le devolvía 403.
/// </summary>
public class ListarEspaciosPreceptorUseCase(
    IPreceptorRepository preceptorRepository,
    IEspacioCurricularRepository espacioRepository)
{
    public async Task<IEnumerable<EspacioCurricularDto>> EjecutarAsync(
        int usuarioId, CancellationToken cancellationToken = default)
    {
        var preceptor = await preceptorRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new BusinessException("No se encontró el preceptor asociado al usuario.");

        return await espacioRepository.ListarPorPreceptorIdAsync(preceptor.Id, cancellationToken);
    }
}
