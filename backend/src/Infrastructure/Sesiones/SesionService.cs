using System.Collections.Concurrent;
using PracticaProfesional.Application.Interfaces;

namespace PracticaProfesional.Infrastructure.Sesiones;

/// <summary>
/// Implementación singleton en memoria.
/// Registra el timestamp del último heartbeat por usuario.
/// </summary>
public class SesionService : ISesionService
{
    private static readonly TimeSpan TiempoInactividad = TimeSpan.FromSeconds(45);

    private readonly ConcurrentDictionary<int, DateTime> _sesiones = new();
    private readonly ConcurrentDictionary<int, DateTime> _forzados = new();

    public void RegistrarActividad(int usuarioId)
        => _sesiones[usuarioId] = DateTime.UtcNow;

    public void RemoverSesion(int usuarioId)
        => _sesiones.TryRemove(usuarioId, out _);

    public IEnumerable<int> ObtenerIdsActivos()
        => _sesiones
            .Where(kv => DateTime.UtcNow - kv.Value <= TiempoInactividad)
            .Select(kv => kv.Key)
            .ToList();

    public void ForzarCierre(int usuarioId)
    {
        // El claim "iat" del JWT tiene precisión de segundos enteros (NumericDate, spec RFC
        // 7519) — si acá se guardara DateTime.UtcNow con sub-segundos, un token emitido dentro
        // del mismo segundo del cierre forzado podía comparar mal en cualquier dirección según
        // los milisegundos exactos (confirmado en pruebas: un login inmediatamente posterior al
        // cierre quedaba rechazado, y el token viejo se colaba). Truncar a segundos + usar ">="
        // en la comparación resuelve el caso ambiguo a favor de exigir un re-login.
        var ahora = DateTime.UtcNow;
        _forzados[usuarioId] = new DateTime(ahora.Year, ahora.Month, ahora.Day, ahora.Hour, ahora.Minute, ahora.Second, DateTimeKind.Utc);
        RemoverSesion(usuarioId);
    }

    public bool FueForzadoDespuesDe(int usuarioId, DateTime momento)
        => _forzados.TryGetValue(usuarioId, out var forzadoEn) && forzadoEn >= momento;
}
