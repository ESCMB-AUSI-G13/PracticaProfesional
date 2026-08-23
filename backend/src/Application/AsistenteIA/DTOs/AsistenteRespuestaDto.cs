namespace PracticaProfesional.Application.AsistenteIA.DTOs;

public record AsistenteRespuestaDto(
    string Respuesta,
    string? HerramientaUsada,
    DateTime GeneradoEn
);
