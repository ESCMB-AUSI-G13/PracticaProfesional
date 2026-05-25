using PracticaProfesional.Application.Alertas.DTOs;
using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Alertas;

public class NotificarVencimientosUseCase(
    ICalendarioAcademicoRepository calendarioRepo,
    IEspacioCurricularRepository espacioRepo,
    IPreceptorRepository preceptorRepo,
    IUsuarioRepository usuarioRepo,
    IAlertaRepository alertaRepo,
    INotificacionRepository notificacionRepo,
    IEmailService emailService)
{
    private const int DiasAnticipacion = 3;

    public async Task<ResumenAlertasDto> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var detalles = new List<string>();
        int alertasGeneradas = 0;
        int emailsEnviados = 0;

        var hoy = DateTime.UtcNow.Date;
        var limite = hoy.AddDays(DiasAnticipacion);

        var eventosProximos = await calendarioRepo.ObtenerProximosAsync(hoy, limite, cancellationToken);
        var preceptores = await preceptorRepo.ListarAsync(cancellationToken);
        var usuariosDireccion = await usuarioRepo.ListarAsync(Rol.Direccion, cancellationToken);

        foreach (var evento in eventosProximos)
        {
            int diasRestantes = (evento.FechaFin.Date - hoy).Days;
            var tipoAlerta = evento.TipoEvento == TipoEvento.FechaLimiteCargaNotas
                ? TipoAlerta.VencimientoCargaNotas
                : TipoAlerta.VencimientoInscripcion;
            var titulo = $"Vencimiento en {diasRestantes} día{(diasRestantes == 1 ? "" : "s")}: {evento.NombreEvento}";

            // ── FechaLimiteCargaNotas → docente del espacio ──────────────────
            if (evento.TipoEvento == TipoEvento.FechaLimiteCargaNotas
                && evento.MateriaId.HasValue && evento.CursoId.HasValue)
            {
                var espacios = await espacioRepo.ListarPorCursoYMateriaAsync(
                    evento.CursoId.Value, evento.MateriaId.Value, cancellationToken);

                foreach (var espacio in espacios)
                {
                    var emailDocente = espacio.Docente?.Usuario?.Email;
                    if (string.IsNullOrWhiteSpace(emailDocente)) continue;

                    bool yaEnviado = await alertaRepo.ExisteAlertaVencimientoHoyAsync(
                        evento.Id, emailDocente, cancellationToken);
                    if (yaEnviado) continue;

                    var nombreDocente = $"{espacio.Docente!.Usuario!.Nombre} {espacio.Docente.Usuario.Apellido}";
                    var alerta = Alerta.Crear(tipoAlerta, emailDocente, evento.NombreEvento, calendarioAcademicoId: evento.Id);
                    await alertaRepo.AgregarAsync(alerta, cancellationToken);
                    alertasGeneradas++;

                    await notificacionRepo.AgregarAsync(
                        Notificacion.Crear(espacio.Docente.UsuarioId, titulo, evento.NombreEvento, tipoAlerta),
                        cancellationToken);

                    if (await EnviarEmailAsync(() => emailService.EnviarAlertaVencimientoAsync(
                            emailDocente, nombreDocente, evento.NombreEvento,
                            evento.FechaFin, diasRestantes, cancellationToken), detalles))
                    {
                        alerta.MarcarEnviada();
                        emailsEnviados++;
                    }

                    detalles.Add($"[{evento.NombreEvento}] → {nombreDocente} — vence en {diasRestantes} días.");
                }
            }

            // ── Todos los eventos → preceptores y dirección ──────────────────
            if (evento.TipoEvento is TipoEvento.InscripcionMateria or TipoEvento.InscripcionExamen
                or TipoEvento.FechaLimiteCargaNotas)
            {
                foreach (var preceptor in preceptores.Where(p => p.Usuario?.Activo == true))
                {
                    var emailP = preceptor.Usuario!.Email;
                    bool yaEnviado = await alertaRepo.ExisteAlertaVencimientoHoyAsync(
                        evento.Id, emailP, cancellationToken);
                    if (yaEnviado) continue;

                    var nombreP = $"{preceptor.Usuario.Nombre} {preceptor.Usuario.Apellido}";
                    var alerta = Alerta.Crear(tipoAlerta, emailP, evento.NombreEvento, calendarioAcademicoId: evento.Id);
                    await alertaRepo.AgregarAsync(alerta, cancellationToken);
                    alertasGeneradas++;

                    await notificacionRepo.AgregarAsync(
                        Notificacion.Crear(preceptor.UsuarioId, titulo, evento.NombreEvento, tipoAlerta),
                        cancellationToken);

                    if (await EnviarEmailAsync(() => emailService.EnviarAlertaVencimientoAsync(
                            emailP, nombreP, evento.NombreEvento,
                            evento.FechaFin, diasRestantes, cancellationToken), detalles))
                        emailsEnviados++;

                    detalles.Add($"[{evento.NombreEvento}] → Preceptor {nombreP} — vence en {diasRestantes} días.");
                }

                foreach (var dir in usuariosDireccion.Where(u => u.Activo))
                {
                    bool yaEnviado = await alertaRepo.ExisteAlertaVencimientoHoyAsync(
                        evento.Id, dir.Email, cancellationToken);
                    if (yaEnviado) continue;

                    var alerta = Alerta.Crear(tipoAlerta, dir.Email, evento.NombreEvento, calendarioAcademicoId: evento.Id);
                    await alertaRepo.AgregarAsync(alerta, cancellationToken);
                    alertasGeneradas++;

                    await notificacionRepo.AgregarAsync(
                        Notificacion.Crear(dir.Id, titulo, evento.NombreEvento, tipoAlerta),
                        cancellationToken);

                    if (await EnviarEmailAsync(() => emailService.EnviarAlertaVencimientoAsync(
                            dir.Email, $"{dir.Nombre} {dir.Apellido}", evento.NombreEvento,
                            evento.FechaFin, diasRestantes, cancellationToken), detalles))
                        emailsEnviados++;

                    detalles.Add($"[{evento.NombreEvento}] → Dirección {dir.Email} — vence en {diasRestantes} días.");
                }
            }
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
