using PracticaProfesional.Application.Alertas.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Alertas;

public class DetectarRiesgoAcademicoUseCase(
    IEstudianteRepository estudianteRepo,
    IInscripcionMateriaRepository inscripcionRepo,
    IAsistenciaRepository asistenciaRepo,
    IPreceptorRepository preceptorRepo,
    IUsuarioRepository usuarioRepo,
    IAlertaRepository alertaRepo,
    INotificacionRepository notificacionRepo,
    IEmailService emailService)
{
    private const double UmbralAsistencia = 0.25;
    private const int DiasInactividad = 30;

    public async Task<ResumenAlertasDto> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var detalles = new List<string>();
        int alertasGeneradas = 0;
        int emailsEnviados = 0;

        var estudiantes = await estudianteRepo.ListarAsync(cancellationToken);
        var estudiantesActivos = estudiantes.Where(e => e.Usuario?.Activo == true).ToList();

        var preceptores = await preceptorRepo.ListarAsync(cancellationToken);
        var usuariosDireccion = await usuarioRepo.ListarAsync(Rol.Direccion, cancellationToken);

        foreach (var estudiante in estudiantesActivos)
        {
            var inscripciones = (await inscripcionRepo.ListarActivasPorEstudianteAsync(
                estudiante.Id, cancellationToken)).ToList();

            // ── Riesgo por asistencia ────────────────────────────────────────
            foreach (var inscripcion in inscripciones)
            {
                var (total, ausentesInjustificados, _) = await asistenciaRepo.ObtenerEstadisticasAsync(
                    estudiante.Id, inscripcion.MateriaId, inscripcion.CursoId, cancellationToken);

                if (total == 0) continue;

                double porcentajeAusencia = (double)ausentesInjustificados / total;
                if (porcentajeAusencia <= UmbralAsistencia) continue;

                bool yaNotificado = await alertaRepo.ExisteAlertaHoyAsync(
                    estudiante.Id, TipoAlerta.RiesgoAsistencia, cancellationToken);
                if (yaNotificado) continue;

                var nombreEstudiante = $"{estudiante.Usuario!.Nombre} {estudiante.Usuario.Apellido}";
                var materia = inscripcion.Materia?.Nombre ?? $"Materia #{inscripcion.MateriaId}";
                int pct = (int)Math.Round(porcentajeAusencia * 100);
                var mensaje = $"Riesgo de asistencia en {materia}: {ausentesInjustificados}/{total} ausencias injustificadas ({pct}%).";
                var titulo = "Alerta: riesgo de pérdida de regularidad";
                var detalle = $"{nombreEstudiante} — {mensaje}";

                var alerta = Alerta.Crear(TipoAlerta.RiesgoAsistencia, estudiante.Usuario.Email, mensaje, estudiante.Id);
                await alertaRepo.AgregarAsync(alerta, cancellationToken);
                alertasGeneradas++;
                detalles.Add(detalle);

                // Notificación interna al estudiante
                await notificacionRepo.AgregarAsync(
                    Notificacion.Crear(estudiante.UsuarioId, titulo, mensaje, TipoAlerta.RiesgoAsistencia),
                    cancellationToken);

                // Notificaciones internas a preceptores y dirección
                var mensajeInterno = $"{nombreEstudiante}: {mensaje}";
                foreach (var preceptor in preceptores.Where(p => p.Usuario?.Activo == true))
                    await notificacionRepo.AgregarAsync(
                        Notificacion.Crear(preceptor.UsuarioId, titulo, mensajeInterno, TipoAlerta.RiesgoAsistencia),
                        cancellationToken);

                foreach (var dir in usuariosDireccion.Where(u => u.Activo))
                    await notificacionRepo.AgregarAsync(
                        Notificacion.Crear(dir.Id, titulo, mensajeInterno, TipoAlerta.RiesgoAsistencia),
                        cancellationToken);

                // Emails
                if (await EnviarEmailAsync(() => emailService.EnviarAlertaRiesgoAcademicoAsync(
                        estudiante.Usuario.Email, nombreEstudiante,
                        "Riesgo de pérdida de regularidad", mensaje, cancellationToken), detalles))
                {
                    alerta.MarcarEnviada();
                    emailsEnviados++;
                }

                foreach (var preceptor in preceptores.Where(p => p.Usuario?.Activo == true))
                    if (await EnviarEmailAsync(() => emailService.EnviarAlertaRiesgoAcademicoAsync(
                            preceptor.Usuario!.Email,
                            $"{preceptor.Usuario.Nombre} {preceptor.Usuario.Apellido}",
                            "Alerta de riesgo académico — estudiante a cargo",
                            detalle, cancellationToken), detalles))
                        emailsEnviados++;

                foreach (var dir in usuariosDireccion.Where(u => u.Activo))
                    if (await EnviarEmailAsync(() => emailService.EnviarAlertaRiesgoAcademicoAsync(
                            dir.Email, $"{dir.Nombre} {dir.Apellido}",
                            "Alerta de riesgo académico — dirección", detalle, cancellationToken), detalles))
                        emailsEnviados++;
            }

            // ── Riesgo por inactividad ───────────────────────────────────────
            bool yaNotificadoInactividad = await alertaRepo.ExisteAlertaHoyAsync(
                estudiante.Id, TipoAlerta.RiesgoInactividad, cancellationToken);
            if (yaNotificadoInactividad) continue;

            var ultimaActividad = await asistenciaRepo.ObtenerUltimaFechaActividadAsync(
                estudiante.Id, cancellationToken);

            bool sinActividad = ultimaActividad == null ||
                (DateTime.UtcNow.Date - ultimaActividad.Value.Date).TotalDays > DiasInactividad;
            if (!sinActividad) continue;

            var nombreEst = $"{estudiante.Usuario!.Nombre} {estudiante.Usuario.Apellido}";
            var mensajeInactividad = ultimaActividad == null
                ? "No se registra asistencia desde el inicio del año lectivo."
                : $"Sin actividad desde el {ultimaActividad.Value:dd/MM/yyyy} (más de {DiasInactividad} días).";
            var tituloInactividad = "Alerta: inactividad académica";
            var detalleInactividad = $"{nombreEst} — {mensajeInactividad}";

            var alertaInactividad = Alerta.Crear(
                TipoAlerta.RiesgoInactividad, estudiante.Usuario.Email, mensajeInactividad, estudiante.Id);
            await alertaRepo.AgregarAsync(alertaInactividad, cancellationToken);
            alertasGeneradas++;
            detalles.Add(detalleInactividad);

            await notificacionRepo.AgregarAsync(
                Notificacion.Crear(estudiante.UsuarioId, tituloInactividad, mensajeInactividad, TipoAlerta.RiesgoInactividad),
                cancellationToken);

            var mensajeInternoInact = $"{nombreEst}: {mensajeInactividad}";
            foreach (var preceptor in preceptores.Where(p => p.Usuario?.Activo == true))
                await notificacionRepo.AgregarAsync(
                    Notificacion.Crear(preceptor.UsuarioId, tituloInactividad, mensajeInternoInact, TipoAlerta.RiesgoInactividad),
                    cancellationToken);

            foreach (var dir in usuariosDireccion.Where(u => u.Activo))
                await notificacionRepo.AgregarAsync(
                    Notificacion.Crear(dir.Id, tituloInactividad, mensajeInternoInact, TipoAlerta.RiesgoInactividad),
                    cancellationToken);

            if (await EnviarEmailAsync(() => emailService.EnviarAlertaRiesgoAcademicoAsync(
                    estudiante.Usuario.Email, nombreEst,
                    "Alerta de inactividad académica", mensajeInactividad, cancellationToken), detalles))
            {
                alertaInactividad.MarcarEnviada();
                emailsEnviados++;
            }

            foreach (var preceptor in preceptores.Where(p => p.Usuario?.Activo == true))
                if (await EnviarEmailAsync(() => emailService.EnviarAlertaRiesgoAcademicoAsync(
                        preceptor.Usuario!.Email,
                        $"{preceptor.Usuario.Nombre} {preceptor.Usuario.Apellido}",
                        "Alerta de inactividad — estudiante a cargo",
                        detalleInactividad, cancellationToken), detalles))
                    emailsEnviados++;

            foreach (var dir in usuariosDireccion.Where(u => u.Activo))
                if (await EnviarEmailAsync(() => emailService.EnviarAlertaRiesgoAcademicoAsync(
                        dir.Email, $"{dir.Nombre} {dir.Apellido}",
                        "Alerta de inactividad académica — dirección", detalleInactividad, cancellationToken), detalles))
                    emailsEnviados++;
        }

        await alertaRepo.GuardarCambiosAsync(cancellationToken);
        await notificacionRepo.GuardarCambiosAsync(cancellationToken);

        return new ResumenAlertasDto
        {
            AlertasGeneradas = alertasGeneradas,
            EmailsEnviados = emailsEnviados,
            Detalles = detalles
        };
    }

    private static async Task<bool> EnviarEmailAsync(Func<Task> envio, List<string> detalles)
    {
        try { await envio(); return true; }
        catch (Exception ex) { detalles.Add($"[EMAIL FALLIDO] {ex.Message}"); return false; }
    }
}
