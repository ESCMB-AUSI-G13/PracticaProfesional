using Microsoft.AspNetCore.Http;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Infrastructure.Seguridad;

public class LogSeguridadService(
    ILogSeguridadRepository repository,
    IHttpContextAccessor httpContextAccessor) : ILogSeguridadService
{
    public async Task RegistrarAsync(
        string email,
        bool exitoso,
        string? motivoFallo = null,
        CancellationToken cancellationToken = default)
    {
        var ctx = httpContextAccessor.HttpContext;

        var ip = ctx?.Request.Headers["X-Forwarded-For"].FirstOrDefault()
              ?? ctx?.Connection.RemoteIpAddress?.ToString()
              ?? "desconocida";

        var userAgent = ctx?.Request.Headers["User-Agent"].ToString() ?? "desconocido";

        // Trunca el User-Agent para no exceder el campo de BD
        if (userAgent.Length > 500) userAgent = userAgent[..500];

        var log = LogSeguridad.Registrar(email, exitoso, ip, userAgent, motivoFallo);
        await repository.AgregarAsync(log, cancellationToken);
    }
}
