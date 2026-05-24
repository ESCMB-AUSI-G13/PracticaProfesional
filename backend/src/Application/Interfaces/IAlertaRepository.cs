using PracticaProfesional.Domain.Entities;
using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Application.Interfaces;

public interface IAlertaRepository
{
    Task AgregarAsync(Alerta alerta, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);

    /// <summary>Evita reenviar la misma alerta de riesgo al mismo estudiante el mismo día.</summary>
    Task<bool> ExisteAlertaHoyAsync(int estudianteId, TipoAlerta tipo, CancellationToken cancellationToken = default);

    /// <summary>Evita reenviar la misma alerta de vencimiento al mismo destinatario el mismo día.</summary>
    Task<bool> ExisteAlertaVencimientoHoyAsync(int calendarioId, string destinatario, CancellationToken cancellationToken = default);

    Task<IEnumerable<Alerta>> ListarAsync(TipoAlerta? tipo = null, bool? enviada = null, CancellationToken cancellationToken = default);
}
