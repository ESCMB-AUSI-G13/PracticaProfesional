using PracticaProfesional.Application.Encuestas.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Encuestas;

public class AgregarPreguntaUseCase(IEncuestaRepository repo)
{
    public async Task<PreguntaEncuestaDto> EjecutarAsync(
        AgregarPreguntaDto dto, CancellationToken ct = default)
    {
        var encuesta = await repo.ObtenerConPreguntasAsync(dto.EncuestaId, ct)
            ?? throw new BusinessException($"No se encontró la encuesta {dto.EncuestaId}.");

        var pregunta = PreguntaEncuesta.Crear(
            dto.EncuestaId, dto.Texto, dto.Orden,
            dto.TipoPregunta, dto.EsObligatoria);

        await repo.AgregarPreguntaAsync(pregunta, ct);

        return new PreguntaEncuestaDto(
            pregunta.Id, pregunta.Texto, pregunta.Orden,
            pregunta.TipoPregunta, pregunta.EsObligatoria);
    }
}
