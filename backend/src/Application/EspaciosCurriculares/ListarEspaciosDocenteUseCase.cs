using PracticaProfesional.Application.EspaciosCurriculares.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.EspaciosCurriculares;

public class ListarEspaciosDocenteUseCase(
    IDocenteRepository docenteRepository,
    IEspacioCurricularRepository espacioRepository)
{
    public async Task<IEnumerable<EspacioCurricularDto>> EjecutarAsync(
        int usuarioId, CancellationToken cancellationToken = default)
    {
        var docente = await docenteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new BusinessException("No se encontró el docente asociado al usuario.");

        var espacios = await espacioRepository.ListarPorDocenteIdAsync(docente.Id, cancellationToken);
        return espacios.Select(ec => CrearEspacioCurricularUseCase.ToDto(ec, ec.Materia, ec.Docente, ec.Curso));
    }
}
