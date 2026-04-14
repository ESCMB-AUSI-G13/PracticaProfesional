using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Auditoria;

public class AuditoriaService(
    IAuditoriaLogRepository repository,
    IHttpContextAccessor httpContextAccessor) : IAuditoriaService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };

    public async Task RegistrarAsync(
        string entidadTipo,
        string entidadId,
        string accion,
        object? valorAnterior = null,
        object? valorNuevo = null,
        CancellationToken cancellationToken = default)
    {
        var claims = httpContextAccessor.HttpContext?.User;

        int? ejecutorId = null;
        var ejecutorEmail = "sistema";

        if (claims is not null)
        {
            var idClaim = claims.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? claims.FindFirstValue("sub");
            if (int.TryParse(idClaim, out var id))
                ejecutorId = id;

            ejecutorEmail = claims.FindFirstValue(ClaimTypes.Email)
                         ?? claims.FindFirstValue("email")
                         ?? "sistema";
        }

        var log = AuditoriaLog.Registrar(
            entidadTipo,
            entidadId,
            accion,
            ejecutorId,
            ejecutorEmail,
            valorAnterior is null ? null : JsonSerializer.Serialize(valorAnterior, JsonOpts),
            valorNuevo   is null ? null : JsonSerializer.Serialize(valorNuevo,   JsonOpts)
        );

        await repository.AgregarAsync(log, cancellationToken);
    }
}
