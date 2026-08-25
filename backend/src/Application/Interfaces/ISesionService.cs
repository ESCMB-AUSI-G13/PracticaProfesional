namespace PracticaProfesional.Application.Interfaces;

/// <summary>
/// Servicio singleton que mantiene en memoria las sesiones activas.
/// Un usuario es "activo" si envió un heartbeat en los últimos 60 segundos.
/// </summary>
public interface ISesionService
{
    void RegistrarActividad(int usuarioId);
    void RemoverSesion(int usuarioId);
    IEnumerable<int> ObtenerIdsActivos();

    /// <summary>
    /// Fuerza el cierre de la sesión de un usuario ahora mismo — a diferencia de
    /// <see cref="RemoverSesion"/> (que solo lo saca de la lista de "activos"), esto además hace
    /// que cualquier JWT emitido ANTES de este momento deje de aceptarse de inmediato (chequeado
    /// en el pipeline de autenticación). Un login posterior emite un token nuevo, válido de
    /// nuevo. Antes Dirección solo podía "ver" quién estaba conectado, sin ninguna forma de
    /// cortarle el acceso a una sesión puntual sin desactivar la cuenta entera
    /// (ver CHECKLIST.md, Tier 7 #38).
    /// </summary>
    void ForzarCierre(int usuarioId);

    /// <summary>
    /// true si el usuario tuvo un cierre forzado en o después del momento indicado (comparación
    /// inclusiva porque el "iat" del JWT solo tiene precisión de segundos: un login en el mismo
    /// segundo del cierre forzado es ambiguo y se resuelve exigiendo re-login).
    /// </summary>
    bool FueForzadoDespuesDe(int usuarioId, DateTime momento);
}
