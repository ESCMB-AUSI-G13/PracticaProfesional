using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using PracticaProfesional.Application.Encuestas.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Encuestas;

/// <summary>
/// Devuelve la primera encuesta activa que el estudiante no haya completado aún.
/// - SatisfaccionGeneral: se muestra a todos los estudiantes.
/// - EvaluacionDocente: solo si el estudiante tiene inscripción activa en la materia de la encuesta.
/// Si no hay pendientes, devuelve null.
/// </summary>
public class ObtenerEncuestaPendienteUseCase(
    IEncuestaRepository          repo,
    IInscripcionMateriaRepository inscripcionRepo,
    IConfiguration               config)
{
    public async Task<EncuestaDto?> EjecutarAsync(int estudianteId, CancellationToken ct = default)
    {
        var activas = await repo.ListarActivasAsync(ct);
        var salt    = config["Encuestas:Salt"] ?? "pp-salt-2026";

        foreach (var encuesta in activas)
        {
            // EvaluacionDocente: solo mostrar si el estudiante cursa esa materia
            if (encuesta.Tipo == TipoEncuesta.EvaluacionDocente && encuesta.MateriaId.HasValue)
            {
                var inscripto = await inscripcionRepo
                    .ExisteInscripcionActivaAsync(estudianteId, encuesta.MateriaId.Value, ct);
                if (!inscripto) continue;
            }

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
