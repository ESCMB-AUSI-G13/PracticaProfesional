using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PracticaProfesional.Application.Alertas;

namespace PracticaProfesional.Infrastructure.BackgroundServices;

public class AlertasBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<AlertasBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Servicio de alertas iniciado. Próxima ejecución: {Fecha:dd/MM/yyyy HH:mm}",
            DateTime.Now + CalcularDemoraSiguienteLunes());

        while (!stoppingToken.IsCancellationRequested)
        {
            var demora = CalcularDemoraSiguienteLunes();
            logger.LogInformation("Alertas programadas para el lunes {Fecha:dd/MM/yyyy} a las 08:00.",
                DateTime.Now + demora);

            await Task.Delay(demora, stoppingToken);

            if (stoppingToken.IsCancellationRequested) break;

            await EjecutarAlertasAsync(stoppingToken);
        }
    }

    private async Task EjecutarAlertasAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Ejecutando alertas automáticas del lunes {Fecha:dd/MM/yyyy}.", DateTime.Now);

        // BackgroundService es singleton, pero los use cases son scoped (usan EF Core).
        // Creamos un scope por cada ejecución para respetar el ciclo de vida de los repositorios.
        using var scope = scopeFactory.CreateScope();

        try
        {
            var detectarRiesgo = scope.ServiceProvider.GetRequiredService<DetectarRiesgoAcademicoUseCase>();
            var resumenRiesgo = await detectarRiesgo.EjecutarAsync(cancellationToken);

            logger.LogInformation(
                "Detección de riesgo completada: {Alertas} alertas, {Emails} emails enviados.",
                resumenRiesgo.AlertasGeneradas, resumenRiesgo.EmailsEnviados);

            foreach (var detalle in resumenRiesgo.Detalles)
                logger.LogInformation("  · {Detalle}", detalle);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al ejecutar detección de riesgo académico.");
        }

        try
        {
            var notificarVencimientos = scope.ServiceProvider.GetRequiredService<NotificarVencimientosUseCase>();
            var resumenVencimientos = await notificarVencimientos.EjecutarAsync(cancellationToken);

            logger.LogInformation(
                "Vencimientos completados: {Alertas} alertas, {Emails} emails enviados.",
                resumenVencimientos.AlertasGeneradas, resumenVencimientos.EmailsEnviados);

            foreach (var detalle in resumenVencimientos.Detalles)
                logger.LogInformation("  · {Detalle}", detalle);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al ejecutar notificación de vencimientos.");
        }
    }

    // Calcula cuánto falta para el próximo lunes a las 08:00.
    // Si hoy es lunes y todavía no son las 08:00, devuelve el tiempo hasta las 08:00 de hoy.
    // Si hoy es lunes y ya pasaron las 08:00, espera hasta el lunes siguiente.
    private static TimeSpan CalcularDemoraSiguienteLunes()
    {
        var ahora = DateTime.Now;
        int diasHastaLunes = ((int)DayOfWeek.Monday - (int)ahora.DayOfWeek + 7) % 7;

        if (diasHastaLunes == 0)
        {
            var objetivo = ahora.Date.AddHours(8);
            if (ahora < objetivo)
                return objetivo - ahora;

            diasHastaLunes = 7;
        }

        return ahora.Date.AddDays(diasHastaLunes).AddHours(8) - ahora;
    }
}
