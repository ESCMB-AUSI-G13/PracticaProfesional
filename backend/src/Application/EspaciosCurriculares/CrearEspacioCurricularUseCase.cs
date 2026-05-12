using PracticaProfesional.Application.EspaciosCurriculares.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.EspaciosCurriculares;

public class CrearEspacioCurricularUseCase(
    IEspacioCurricularRepository repository,
    IMateriaRepository materiaRepository,
    IDocenteRepository docenteRepository,
    ICursoRepository cursoRepository,
    IAuditoriaService auditoria)
{
    public async Task<EspacioCurricularDto> EjecutarAsync(
        CrearEspacioCurricularDto dto, CancellationToken cancellationToken = default)
    {
        var materia  = await materiaRepository.ObtenerPorIdAsync(dto.MateriaId, cancellationToken)
            ?? throw new BusinessException($"No se encontró la materia con Id {dto.MateriaId}.");
        var docente  = await docenteRepository.ObtenerPorUsuarioIdAsync(dto.UsuarioDocenteId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el docente con UsuarioId {dto.UsuarioDocenteId}.");

        if (await repository.ExisteAsync(dto.MateriaId, docente.Id, dto.CursoId, cancellationToken))
            throw new BusinessException("Ya existe una cátedra con esa combinación de Materia, Docente y Curso.");
        var curso    = await cursoRepository.ObtenerPorIdAsync(dto.CursoId, cancellationToken)
            ?? throw new BusinessException($"No se encontró el curso con Id {dto.CursoId}.");

        var ec = EspacioCurricular.Crear(materia.Id, docente.Id, curso.Id);
        await repository.AgregarAsync(ec, cancellationToken);

        await auditoria.RegistrarAsync("EspacioCurricular", ec.Id.ToString(), "CREAR",
            valorAnterior: null,
            valorNuevo: new { ec.MateriaId, ec.DocenteId, ec.CursoId },
            cancellationToken: cancellationToken);

        return ToDto(ec, materia, docente, curso);
    }

    internal static EspacioCurricularDto ToDto(
        EspacioCurricular ec, Materia m, Docente d, Curso c) => new(
        ec.Id,
        m.Id, m.Nombre, m.Codigo, m.Anio,
        m.CarreraId, m.Carrera?.Nombre ?? string.Empty,
        d.Id, $"{d.Usuario.Nombre} {d.Usuario.Apellido}",
        c.Id, c.Anio, c.AnioLectivo, c.Comision);
}
