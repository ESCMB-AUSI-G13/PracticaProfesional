using Microsoft.Extensions.Configuration;
using PracticaProfesional.Application.Encuestas.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Application.Encuestas;

/// <summary>
/// Registra la respuesta anónima de un estudiante a una encuesta.
/// El token SHA-256 desasocia la identidad: la respuesta no tiene FK al alumno.
/// </summary>
public class ResponderEncuestaUseCase(
    IEncuestaRepository  repo,
    IEstudianteRepository estudianteRepo,
    IConfiguration       config)
{
    public async Task EjecutarAsync(
        int usuarioId, ResponderEncuestaDto dto, CancellationToken ct = default)
    {
        var estudiante = await estudianteRepo.ObtenerPorUsuarioIdAsync(usuarioId, ct)
            ?? throw new BusinessException("No se encontró el perfil de estudiante.");

        var encuesta = await repo.ObtenerConPreguntasAsync(dto.EncuestaId, ct)
            ?? throw new BusinessException($"Encuesta {dto.EncuestaId} no encontrada.");

        if (!encuesta.Activa)
            throw new BusinessException("La encuesta no está activa.");

        var salt  = config["Encuestas:Salt"] ?? "pp-salt-2026";
        var token = ObtenerEncuestaPendienteUseCase.ComputarToken(estudiante.Id, encuesta.Id, salt);

        if (await repo.TokenYaExisteAsync(token, encuesta.Id, ct))
            throw new BusinessException("Ya completaste esta encuesta.", 409);

        // Validar que las preguntas obligatorias estén respondidas
        var preguntasObligatorias = encuesta.Preguntas
            .Where(p => p.EsObligatoria)
            .Select(p => p.Id)
            .ToHashSet();

        var respondidas = dto.Items.Select(i => i.PreguntaId).ToHashSet();
        var faltantes   = preguntasObligatorias.Except(respondidas).ToList();

        if (faltantes.Count > 0)
            throw new BusinessException("Hay preguntas obligatorias sin responder.");

        // Validar rangos Likert
        foreach (var item in dto.Items)
        {
            var pregunta = encuesta.Preguntas.FirstOrDefault(p => p.Id == item.PreguntaId);
            if (pregunta is null) continue;

            if (pregunta.TipoPregunta == TipoPregunta.EscalaLikert)
            {
                if (item.ValorNumerico is null or < 1 or > 5)
                    throw new BusinessException($"La pregunta '{pregunta.Texto}' requiere un valor entre 1 y 5.");
            }
        }

        // Crear respuesta anónima (sin FK al estudiante)
        var respuesta = RespuestaEncuesta.Crear(encuesta.Id);
        await repo.AgregarRespuestaAsync(respuesta, ct);

        foreach (var item in dto.Items)
        {
            var itemEntidad = ItemRespuesta.Crear(
                respuesta.Id, item.PreguntaId,
                item.ValorNumerico, item.TextoLibre);
            respuesta.Items.Add(itemEntidad);
        }
        await repo.GuardarCambiosAsync(ct);

        // Registrar token para evitar doble respuesta (no revela identidad)
        await repo.RegistrarCompletadaAsync(
            EncuestaCompletada.Crear(token, encuesta.Id), ct);
    }
}
