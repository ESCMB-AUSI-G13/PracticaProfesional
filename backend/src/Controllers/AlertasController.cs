using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PracticaProfesional.Application.Alertas;
using PracticaProfesional.Application.Alertas.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Controllers;

[ApiController]
[Route("api/alertas")]
[Authorize]
public class AlertasController(
    DetectarRiesgoAcademicoUseCase detectarRiesgoUseCase,
    NotificarVencimientosUseCase notificarVencimientosUseCase,
    ListarAlertasUseCase listarUseCase,
    IEmailService emailService,
    IWebHostEnvironment env) : ControllerBase
{
    /// <summary>
    /// Detecta estudiantes en riesgo académico (asistencia y/o inactividad)
    /// y envía notificaciones al estudiante, preceptores y dirección.
    /// Roles: Preceptor, Direccion.
    /// </summary>
    [HttpPost("detectar-riesgo")]
    [Authorize(Roles = "Preceptor,Direccion")]
    public async Task<ActionResult<ResumenAlertasDto>> DetectarRiesgo(CancellationToken cancellationToken)
    {
        var resultado = await detectarRiesgoUseCase.EjecutarAsync(cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Notifica sobre eventos del calendario académico próximos a vencer (próximos 3 días).
    /// Roles: Preceptor, Direccion.
    /// </summary>
    [HttpPost("vencimientos")]
    [Authorize(Roles = "Preceptor,Direccion")]
    public async Task<ActionResult<ResumenAlertasDto>> NotificarVencimientos(CancellationToken cancellationToken)
    {
        var resultado = await notificarVencimientosUseCase.EjecutarAsync(cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Solo disponible en Development. Envía un email de prueba para verificar la integración con ACS.
    /// </summary>
    [HttpPost("test-email")]
    [AllowAnonymous]
    public async Task<IActionResult> TestEmail([FromQuery] string destinatario, CancellationToken cancellationToken)
    {
        if (!env.IsDevelopment())
            return NotFound();

        await emailService.EnviarAlertaRiesgoAcademicoAsync(
            destinatario,
            "Usuario de Prueba",
            "Test de integración — Riesgo de asistencia",
            "Este es un email de prueba generado desde el endpoint /api/alertas/test-email. Si lo recibiste, la integración con Azure Communication Services funciona correctamente.",
            cancellationToken);

        await emailService.EnviarAlertaVencimientoAsync(
            destinatario,
            "Usuario de Prueba",
            "Cierre de período de inscripción a materias (TEST)",
            DateTime.Today.AddDays(2),
            diasRestantes: 2,
            cancellationToken);

        return Ok(new { mensaje = $"Emails de prueba enviados a {destinatario}. Revisá la bandeja de entrada (y spam)." });
    }

    /// <summary>
    /// Lista las alertas registradas, con filtros opcionales por tipo y estado de envío.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Preceptor,Direccion")]
    public async Task<ActionResult<IEnumerable<AlertaDto>>> Listar(
        [FromQuery] TipoAlerta? tipo,
        [FromQuery] bool? enviada,
        CancellationToken cancellationToken)
    {
        var alertas = await listarUseCase.EjecutarAsync(tipo, enviada, cancellationToken);
        return Ok(alertas);
    }
}
