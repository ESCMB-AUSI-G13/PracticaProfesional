using Google.GenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Infrastructure.AsistenteIA;

/// <summary>
/// Cliente del modelo Gemini (tier gratuito) vía el SDK oficial Google.GenAI.
/// Usa la integración con Microsoft.Extensions.AI (AsIChatClient + UseFunctionInvocation)
/// para que el loop de tool-calling lo maneje la librería en vez de código propio.
/// </summary>
public class AsistenteIAService(IConfiguration configuration, ILogger<AsistenteIAService> logger)
    : IAsistenteIAService
{
    private const int MaxOutputTokens = 1024;

    public async Task<AsistenteIARespuesta> PreguntarAsync(
        string systemPrompt,
        string pregunta,
        IReadOnlyList<AITool> herramientas,
        CancellationToken cancellationToken = default)
    {
        var apiKey = configuration["GeminiIA:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new BusinessException("El asistente de IA no está configurado.", 503);

        var modelo = configuration["GeminiIA:Model"];
        if (string.IsNullOrWhiteSpace(modelo))
            modelo = "gemini-3.6-flash";

        try
        {
            IChatClient baseClient = new Client(apiKey: apiKey).AsIChatClient(modelo);
            IChatClient chatClient = baseClient.AsBuilder().UseFunctionInvocation().Build();

            List<ChatMessage> mensajes =
            [
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, pregunta)
            ];

            var opciones = new ChatOptions
            {
                Tools = [.. herramientas],
                MaxOutputTokens = MaxOutputTokens
            };

            var respuesta = await chatClient.GetResponseAsync(mensajes, opciones, cancellationToken);

            var herramientaUsada = respuesta.Messages
                .SelectMany(m => m.Contents)
                .OfType<FunctionCallContent>()
                .Select(fc => fc.Name)
                .LastOrDefault();

            return new AsistenteIARespuesta(respuesta.Text, herramientaUsada);
        }
        catch (Exception ex) when (ex is not BusinessException)
        {
            logger.LogError(ex, "Error al llamar al asistente de IA (Gemini).");
            throw new BusinessException("El asistente de IA no está disponible en este momento.", 503);
        }
    }
}
