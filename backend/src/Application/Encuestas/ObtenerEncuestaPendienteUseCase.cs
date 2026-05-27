using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using PracticaProfesional.Application.Encuestas.DTOs;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Application.Encuestas;

/// <summary>
/// Devuelve la primera encuesta activa que el estudiante no haya completado aún.
/// Si no hay pendientes, devuelve null → puede proceder con la inscripción.
/// </summary>
public class ObtenerEncuestaPendienteUseCase(
    IEncuestaRepository  repo,
    IConfiguration       config)
{
    public async Task<EncuestaDto?> EjecutarAsync(int estudianteId, CancellationToken ct = default)
    {
        var activas = await repo.ListarActivasAsync(ct);
        var salt    = config["Encuestas:Salt"] ?? "pp-salt-2026";

        foreach (var encuesta in activas)
        {
            var token = ComputarToken(estudianteId, encuesta.Id, salt);
            if (!await repo.TokenYaExisteAsync(token, encuesta.Id, ct))
                return ListarEncuestasUseCase.ToDto(encuesta);
        }

        return null;
    }

    internal static string ComputarToken(int estudianteId, int encuestaId, string salt)
    {
        var raw   = $"{estudianteId}|{encuestaId}|{salt}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
