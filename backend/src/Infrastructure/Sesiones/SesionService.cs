using System.Collections.Concurrent;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Infrastructure.Sesiones;

/// <summary>
/// Implementación singleton en memoria.
/// Registra el timestamp del último heartbeat por usuario.
/// </summary>
public class SesionService : ISesionService
{
    private static readonly TimeSpan TiempoInactividad = TimeSpan.FromSeconds(60);

    private readonly ConcurrentDictionary<int, DateTime> _sesiones = new();

    public void RegistrarActividad(int usuarioId)
        => _sesiones[usuarioId] = DateTime.UtcNow;

    public void RemoverSesion(int usuarioId)
        => _sesiones.TryRemove(usuarioId, out _);

    public IEnumerable<int> ObtenerIdsActivos()
        => _sesiones
            .Where(kv => DateTime.UtcNow - kv.Value <= TiempoInactividad)
            .Select(kv => kv.Key)
            .ToList();
}
