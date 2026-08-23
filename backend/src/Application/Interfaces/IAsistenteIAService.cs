using Microsoft.Extensions.AI;

namespace PracticaProfesional.Application.Interfaces;

/// <summary>
/// Cliente del proveedor de IA (Gemini, tier gratuito) con soporte de tool-calling.
/// No conoce nada de Reportes ni de negocio: solo habla el protocolo de chat/tools.
/// </summary>
public interface IAsistenteIAService
{
    Task<AsistenteIARespuesta> PreguntarAsync(
        string systemPrompt,
        string pregunta,
        IReadOnlyList<AITool> herramientas,
        CancellationToken cancellationToken = default);
}

/// <summary>Respuesta del proveedor de IA junto con el nombre de la última herramienta invocada (si hubo alguna).</summary>
public record AsistenteIARespuesta(string Texto, string? HerramientaUsada);