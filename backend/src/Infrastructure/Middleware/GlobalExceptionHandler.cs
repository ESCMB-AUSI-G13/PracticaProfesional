using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Infrastructure.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            BusinessException bex => (bex.StatusCode, "Error de negocio"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "No autorizado"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflicto de negocio"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
