using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IEncuestaRepository
{
    // ── Encuestas ─────────────────────────────────────────────────────────────
    Task<List<Encuesta>> ListarActivasAsync(CancellationToken ct = default);
    Task<List<Encuesta>> ListarTodasAsync(CancellationToken ct = default);
    Task<Encuesta?>      ObtenerConPreguntasAsync(int encuestaId, CancellationToken ct = default);
    Task                 AgregarAsync(Encuesta encuesta, CancellationToken ct = default);
    Task                 GuardarCambiosAsync(CancellationToken ct = default);

    // ── Preguntas ─────────────────────────────────────────────────────────────
    Task AgregarPreguntaAsync(PreguntaEncuesta pregunta, CancellationToken ct = default);

    // ── Respuestas (anónimas) ─────────────────────────────────────────────────
    Task AgregarRespuestaAsync(RespuestaEncuesta respuesta, CancellationToken ct = default);

    // ── Control de deduplicación vía token SHA-256 ────────────────────────────
    Task<bool> TokenYaExisteAsync(string tokenAnonimo, int encuestaId, CancellationToken ct = default);
    Task       RegistrarCompletadaAsync(EncuestaCompletada completada, CancellationToken ct = default);

    // ── Reportes ──────────────────────────────────────────────────────────────
    Task<List<RespuestaEncuesta>> ObtenerRespuestasConItemsAsync(
        int encuestaId, CancellationToken ct = default);
}
