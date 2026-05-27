using PracticaProfesional.Application.Encuestas.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Encuestas;

public class CrearEncuestaDocenteUseCase(
    IDocenteRepository           docenteRepo,
    IEspacioCurricularRepository espacioRepo,
    IEncuestaRepository          encuestaRepo)
{
    public async Task<EncuestaDto> EjecutarAsync(
        int usuarioId, CrearEncuestaDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo))
            throw new BusinessException("El título es obligatorio.");

        if (dto.MateriaId is null)
            throw new BusinessException("Debe seleccionar una materia.");

        var docente = await docenteRepo.ObtenerPorUsuarioIdAsync(usuarioId, ct)
            ?? throw new BusinessException("Docente no encontrado.");

        var espacios   = await espacioRepo.ListarPorDocenteIdAsync(docente.Id, ct);
        var materiaIds = espacios.Select(e => e.MateriaId).ToHashSet();

        if (!materiaIds.Contains(dto.MateriaId.Value))
            throw new BusinessException("La materia seleccionada no está asignada a su perfil.", 403);

        var encuesta = Encuesta.Crear(
            dto.Titulo,
            TipoEncuesta.EvaluacionDocente,
            dto.CicloLectivo,
            dto.Descripcion,
            dto.MateriaId);

        await encuestaRepo.AgregarAsync(encuesta, ct);
        return ListarEncuestasUseCase.ToDto(encuesta);
    }
}
