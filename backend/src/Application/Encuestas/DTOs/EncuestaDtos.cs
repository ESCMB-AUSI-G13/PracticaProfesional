using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Encuestas.DTOs;

// ── Requests ──────────────────────────────────────────────────────────────────

public record CrearEncuestaDto(
    string       Titulo,
    string?      Descripcion,
    TipoEncuesta Tipo,
    int          CicloLectivo,
    int?         MateriaId);

public record AgregarPreguntaDto(
    int         EncuestaId,
    string      Texto,
    int         Orden,
    TipoPregunta TipoPregunta,
    bool        EsObligatoria = true);

public record ItemRespuestaDto(
    int     PreguntaId,
    int?    ValorNumerico,
    string? TextoLibre);

public record ResponderEncuestaDto(
    int                    EncuestaId,
    List<ItemRespuestaDto> Items);

// ── Docente — materia simple ──────────────────────────────────────────────────

public record MateriaEncuestaDto(int Id, string Nombre, string Codigo);

// ── Responses ─────────────────────────────────────────────────────────────────

public record PreguntaEncuestaDto(
    int          Id,
    string       Texto,
    int          Orden,
    TipoPregunta TipoPregunta,
    bool         EsObligatoria);

public record EncuestaDto(
    int                      Id,
    string                   Titulo,
    string?                  Descripcion,
    TipoEncuesta             Tipo,
    int?                     MateriaId,
    string?                  MateriaNombre,
    int                      CicloLectivo,
    bool                     Activa,
    DateTime                 FechaCreacion,
    List<PreguntaEncuestaDto> Preguntas);
