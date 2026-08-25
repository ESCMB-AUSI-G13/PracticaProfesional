using PracticaProfesional.Application.Cursos.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Cursos;

public class ModificarCursoUseCase(
    ICursoRepository cursoRepository,
    IPreceptorRepository preceptorRepository,
    IAuditoriaService auditoria)
{
    public async Task<CursoDto> EjecutarAsync(int id, ModificarCursoDto dto, CancellationToken cancellationToken = default)
    {
        var curso = await cursoRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new BusinessException($"No se encontró el curso con Id {id}.");

        if (await cursoRepository.ExistePorAnioYComisionExcluyendoAsync(curso.Anio, curso.AnioLectivo, dto.Comision, curso.CarreraId, id, cancellationToken))
            throw new BusinessException($"Ya existe otro curso con la comisión '{dto.Comision.ToUpperInvariant()}' en el año {curso.Anio}, {curso.AnioLectivo}° año para la misma carrera.");

        var preceptor = await preceptorRepository.ObtenerPorUsuarioIdAsync(dto.PreceptorUsuarioId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el preceptor con Usuario Id {dto.PreceptorUsuarioId}.");

        var anterior = new { curso.Comision, curso.Cupo, curso.PreceptorId };
        curso.Modificar(dto.Comision, dto.Cupo, preceptor.Id);
        await cursoRepository.GuardarCambiosAsync(cancellationToken);

        await auditoria.RegistrarAsync("Curso", curso.Id.ToString(), "MODIFICAR",
            valorAnterior: anterior,
            valorNuevo: new { curso.Comision, curso.Cupo, curso.PreceptorId },
            cancellationToken: cancellationToken);

        return CrearCursoUseCase.ToDto(curso, preceptor);
    }
}
