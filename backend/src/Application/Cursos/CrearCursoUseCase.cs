using PracticaProfesional.Application.Cursos.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Cursos;

public class CrearCursoUseCase(
    ICursoRepository cursoRepository,
    IPreceptorRepository preceptorRepository,
    IAuditoriaService auditoria)
{
    public async Task<CursoDto> EjecutarAsync(CrearCursoDto dto, CancellationToken cancellationToken = default)
    {
        if (await cursoRepository.ExistePorAnioYComisionAsync(dto.Anio, dto.AnioLectivo, dto.Comision, cancellationToken))
            throw new BusinessException($"Ya existe un curso para el año {dto.Anio}, {dto.AnioLectivo}° año y comisión '{dto.Comision.ToUpperInvariant()}'.");

        var preceptor = await preceptorRepository.ObtenerPorUsuarioIdAsync(dto.PreceptorId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el preceptor con Id {dto.PreceptorId}.");

        var curso = Curso.Crear(dto.Anio, dto.AnioLectivo, dto.Comision, dto.Cupo, preceptor.Id);
        await cursoRepository.AgregarAsync(curso, cancellationToken);

        await auditoria.RegistrarAsync("Curso", curso.Id.ToString(), "CREAR",
            valorAnterior: null,
            valorNuevo: new { curso.Anio, curso.AnioLectivo, curso.Comision, curso.Cupo, PreceptorId = preceptor.Id },
            cancellationToken: cancellationToken);

        return ToDto(curso, preceptor);
    }

    internal static CursoDto ToDto(Curso c, Domain.Entities.Preceptor p) => new(
        c.Id, c.Anio, c.AnioLectivo, c.Comision, c.Cupo, c.Estado.ToString(),
        p.Id, $"{p.Usuario.Nombre} {p.Usuario.Apellido}");
}
